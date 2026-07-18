using System.Diagnostics;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Infrastructure.Usb;

public sealed class SafeUsbWriteWorkflowService : IUsbWriteWorkflowService
{
    private const long Fat32SingleFileLimitBytes = 4L * 1024L * 1024L * 1024L;
    private const long MinimumMarginBytes = 256L * 1024L * 1024L;

    private readonly IUsbDriveService _usbDriveService;
    private readonly IWwtService _wwtService;
    private readonly IExternalToolManager _externalToolManager;
    private readonly IDevicePermissionService _devicePermissionService;
    private readonly ITaskManager _taskManager;
    private readonly INotificationService _notificationService;
    private readonly IWiiUsbReadinessService? _readinessService;

    public SafeUsbWriteWorkflowService(
        IUsbDriveService usbDriveService,
        IWwtService wwtService,
        IExternalToolManager externalToolManager,
        IDevicePermissionService devicePermissionService,
        ITaskManager taskManager,
        INotificationService notificationService,
        IWiiUsbReadinessService? readinessService = null)
    {
        _usbDriveService = usbDriveService;
        _wwtService = wwtService;
        _externalToolManager = externalToolManager;
        _devicePermissionService = devicePermissionService;
        _taskManager = taskManager;
        _notificationService = notificationService;
        _readinessService = readinessService;
    }

    public async Task<UsbWritePreview> CreatePreviewAsync(
        string sourcePath,
        Guid targetDriveId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        cancellationToken.ThrowIfCancellationRequested();

        var warnings = new List<string>();
        var drive = await FindDriveAsync(targetDriveId, cancellationToken);
        var sourceExists = File.Exists(sourcePath);
        var sourceSize = sourceExists ? new FileInfo(sourcePath).Length : 0;
        var requiredBytes = CalculateRequiredBytes(sourceSize);
        var toolStatus = await _externalToolManager.CheckAsync("wiimms-wwt", cancellationToken);
        var permissionStatus = await CheckPermissionAsync(toolStatus.ExecutablePath, drive, cancellationToken);
        var blockReason = GetBlockReason(sourcePath, sourceExists, sourceSize, requiredBytes, drive, toolStatus, permissionStatus);
        var gameId = TryGetGameId(sourcePath);

        if (drive.FileSystem == UsbDriveFileSystem.Fat32 && sourceSize >= Fat32SingleFileLimitBytes)
        {
            warnings.Add("usbWrite.fat32SplitRequired");
        }

        if (gameId is not null)
        {
            var duplicateWarning = await TryDetectDuplicateAsync(drive, gameId, cancellationToken);
            if (duplicateWarning is not null)
            {
                blockReason ??= duplicateWarning;
            }
        }

        return new UsbWritePreview(
            sourcePath,
            gameId,
            drive.Id,
            drive.Label,
            drive.DevicePath,
            drive.MountPoint,
            drive.FileSystem,
            sourceSize,
            requiredBytes,
            drive.FreeSpace,
            drive.FileSystem == UsbDriveFileSystem.Fat32 && sourceSize >= Fat32SingleFileLimitBytes,
            UserConfirmationRequired: true,
            IsBlocked: blockReason is not null,
            blockReason,
            permissionStatus,
            warnings);
    }

    public async Task<UsbWriteResult> ExecuteAsync(
        UsbWriteExecutionRequest request,
        IProgress<WiimmsOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!request.UserConfirmed)
        {
            return Failed(request.Preview, "usbWrite.confirmationRequired", TimeSpan.Zero);
        }

        if (request.Preview.IsBlocked)
        {
            return Failed(request.Preview, request.Preview.BlockReasonKey ?? "usbWrite.blocked", TimeSpan.Zero);
        }

        var drive = await FindDriveAsync(request.Preview.TargetDriveId, cancellationToken);
        var executionBlock = GetExecutionBlockReason(request.Preview, drive);
        if (executionBlock is not null)
        {
            return Failed(request.Preview, executionBlock, TimeSpan.Zero);
        }

        var taskId = await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Import, "Escrevendo jogo no USB", request.Preview.SourcePath),
            cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            progress?.Report(new WiimmsOperationProgress(null, "Iniciando escrita USB", request.Preview.SourcePath, null, null, null, null, IsIndeterminate: true));
            await _taskManager.UpdateProgressAsync(taskId, 0.1, ManagedTaskStatus.Running, cancellationToken);
            await _wwtService.AddGameAsync(
                drive,
                request.Preview.SourcePath,
                cancellationToken,
                CreateProgressReporter(taskId, progress, cancellationToken),
                publishTask: false);
            await _taskManager.UpdateProgressAsync(taskId, 0.9, ManagedTaskStatus.Running, cancellationToken);
            var readiness = await CheckPostWriteReadinessAsync(drive.Id, cancellationToken);
            if (readiness?.State == ReadinessState.NotReady)
            {
                stopwatch.Stop();
                await _taskManager.FailAsync(taskId, "usbWrite.postWriteValidationFailed", CancellationToken.None);
                await _notificationService.PublishAsync(
                    NotificationMessage.Create(NotificationType.Error, "Falha na validação pós-escrita", request.Preview.SourcePath, isPersistent: true, groupKey: taskId.ToString("N")),
                    CancellationToken.None);
                return Failed(
                    request.Preview,
                    "usbWrite.postWriteValidationFailed",
                    stopwatch.Elapsed,
                    readiness);
            }

            stopwatch.Stop();
            await _taskManager.CompleteAsync(taskId, cancellationToken);
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Success, "Escrita USB concluída", request.Preview.SourcePath, groupKey: taskId.ToString("N")),
                cancellationToken);
            progress?.Report(new WiimmsOperationProgress(100, "Escrita USB concluída", request.Preview.SourcePath, null, null, null, TimeSpan.Zero, IsIndeterminate: false));
            return new UsbWriteResult(
                true,
                request.Preview.GameId,
                drive.Id,
                null,
                stopwatch.Elapsed,
                readiness?.State,
                readiness?.Items);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            await _taskManager.CancelAsync(taskId, CancellationToken.None);
            return Failed(request.Preview, "usbWrite.cancelled", stopwatch.Elapsed);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            await _taskManager.FailAsync(taskId, "usbWrite.failed", CancellationToken.None);
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Error, "Falha na escrita USB", request.Preview.SourcePath, isPersistent: true, groupKey: taskId.ToString("N")),
                CancellationToken.None);
            return Failed(request.Preview, "usbWrite.failed", stopwatch.Elapsed);
        }
    }

    private async Task<UsbDrive> FindDriveAsync(Guid driveId, CancellationToken cancellationToken)
    {
        var drives = await _usbDriveService.DetectAsync(cancellationToken);
        return drives.FirstOrDefault(drive => drive.Id == driveId)
            ?? throw new InvalidOperationException($"USB drive {driveId} was not found.");
    }

    private async Task<DevicePermissionStatus?> CheckPermissionAsync(
        string? executablePath,
        UsbDrive drive,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        return await _devicePermissionService.CheckAsync(
            new DevicePermissionRequest("wwt", executablePath, drive.DevicePath, PrivilegedDeviceOperation.AddWbfsGame),
            cancellationToken);
    }

    private static string? GetBlockReason(
        string sourcePath,
        bool sourceExists,
        long sourceSize,
        long requiredBytes,
        UsbDrive drive,
        ExternalToolStatus toolStatus,
        DevicePermissionStatus? permissionStatus)
    {
        if (!sourceExists)
        {
            return "usbWrite.sourceMissing";
        }

        if (sourceSize <= 0)
        {
            return "usbWrite.sourceEmpty";
        }

        if (!IsSupportedSource(sourcePath))
        {
            return "usbWrite.sourceUnsupported";
        }

        if (!drive.IsMounted)
        {
            return "usbWrite.targetNotMounted";
        }

        if (IsSystemTarget(drive))
        {
            return "usbWrite.systemDiskBlocked";
        }

        if (!drive.IsWritable)
        {
            return "usbWrite.targetReadOnly";
        }

        if (!IsSupportedFileSystem(drive.FileSystem))
        {
            return "usbWrite.filesystemUnsupported";
        }

        if (drive.FreeSpace < requiredBytes)
        {
            return "usbWrite.insufficientSpace";
        }

        if (toolStatus.State is not (ExternalToolState.Installed or ExternalToolState.Configured)
            || string.IsNullOrWhiteSpace(toolStatus.ExecutablePath))
        {
            return "usbWrite.wwtUnavailable";
        }

        if (permissionStatus?.State is PrivilegedAccessState.PolicyMissing
            or PrivilegedAccessState.HelperMissing
            or PrivilegedAccessState.PermissionDenied
            or PrivilegedAccessState.Failed
            or PrivilegedAccessState.Unsupported)
        {
            return "usbWrite.permissionDenied";
        }

        return null;
    }

    private static string? GetExecutionBlockReason(UsbWritePreview preview, UsbDrive drive)
    {
        if (!string.Equals(preview.TargetDevicePath, drive.DevicePath, StringComparison.OrdinalIgnoreCase)
            || preview.TargetFileSystem != drive.FileSystem)
        {
            return "usbWrite.targetChanged";
        }

        if (IsSystemTarget(drive))
        {
            return "usbWrite.systemDiskBlocked";
        }

        return null;
    }

    private async Task<WiiUsbReadinessResult?> CheckPostWriteReadinessAsync(Guid driveId, CancellationToken cancellationToken)
    {
        if (_readinessService is null)
        {
            return null;
        }

        return await _readinessService.CheckAsync(driveId, cancellationToken);
    }

    private IProgress<WiimmsOperationProgress> CreateProgressReporter(
        Guid taskId,
        IProgress<WiimmsOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        return new Progress<WiimmsOperationProgress>(update =>
        {
            progress?.Report(update);
            if (update.Percentage is null)
            {
                return;
            }

            var taskProgress = Math.Clamp(update.Percentage.Value / 100d, 0.1, 0.89);
            try
            {
                _taskManager
                    .UpdateProgressAsync(taskId, taskProgress, ManagedTaskStatus.Running, cancellationToken)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.WriteLine($"Failed to update USB write progress: {exception.Message}");
            }
        });
    }

    private async Task<string?> TryDetectDuplicateAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken)
    {
        try
        {
            var games = await _wwtService.ListGamesAsync(drive, cancellationToken);
            return games.Any(game => string.Equals(game.GameId.Value, gameId, StringComparison.OrdinalIgnoreCase))
                ? "usbWrite.duplicateGame"
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static long CalculateRequiredBytes(long sourceSize)
    {
        var margin = Math.Max(MinimumMarginBytes, (long)(sourceSize * 0.05));
        return sourceSize + margin;
    }

    private static bool IsSupportedSource(string sourcePath)
    {
        if (sourcePath.EndsWith(".nkit.iso", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Path.GetExtension(sourcePath).ToLowerInvariant() is ".iso" or ".wbfs" or ".rvz";
    }

    private static bool IsSupportedFileSystem(UsbDriveFileSystem fileSystem)
    {
        return fileSystem is UsbDriveFileSystem.Wbfs or UsbDriveFileSystem.Fat32 or UsbDriveFileSystem.Ntfs;
    }

    private static string? TryGetGameId(string sourcePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourcePath);
        if (fileName.EndsWith(".nkit", StringComparison.OrdinalIgnoreCase))
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
        }

        return fileName.Length >= 6 && fileName.Take(6).All(char.IsLetterOrDigit)
            ? fileName[..6].ToUpperInvariant()
            : null;
    }

    private static bool IsSystemTarget(UsbDrive drive)
    {
        var mountPoint = NormalizePath(drive.MountPoint);
        if (string.IsNullOrWhiteSpace(mountPoint))
        {
            return false;
        }

        if (mountPoint is "/" or "/boot" or "/boot/efi" or "/System/Volumes/Data")
        {
            return true;
        }

        var userProfile = NormalizePath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        if (!string.IsNullOrWhiteSpace(userProfile)
            && string.Equals(mountPoint, userProfile, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (OperatingSystem.IsWindows())
        {
            var systemRoot = NormalizePath(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) ?? string.Empty);
            return !string.IsNullOrWhiteSpace(systemRoot)
                && string.Equals(NormalizePath(Path.GetPathRoot(mountPoint) ?? mountPoint), systemRoot, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var trimmed = path.Trim();
        var root = Path.GetPathRoot(trimmed);
        if (string.Equals(trimmed, root, StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return trimmed.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static UsbWriteResult Failed(
        UsbWritePreview preview,
        string errorMessageKey,
        TimeSpan duration,
        WiiUsbReadinessResult? readiness = null)
    {
        return new UsbWriteResult(
            false,
            preview.GameId,
            preview.TargetDriveId,
            errorMessageKey,
            duration,
            readiness?.State,
            readiness?.Items);
    }
}
