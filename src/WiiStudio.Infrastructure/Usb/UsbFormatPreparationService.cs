using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Usb;

public interface IUsbPreparationFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    bool CanWriteDirectory(string path);
}

public interface IWiimmsFormatCommandExecutor
{
    bool HasCataloguedFormatWbfsCommand { get; }

    Task ExecuteFormatWbfsAsync(
        UsbFormatPreview preview,
        IProgress<UsbFormatProgress>? progress,
        CancellationToken cancellationToken);

    Task<bool> ValidateWbfsAsync(UsbFormatPreview preview, CancellationToken cancellationToken);
}

public sealed class UsbFormatPreparationService : IUsbFormatPreparationService
{
    public const string WbfsConfirmationPhrase = "FORMATAR WBFS";

    private readonly IUsbDeviceProvider _usbDeviceProvider;
    private readonly IExternalToolManager _externalToolManager;
    private readonly IDevicePermissionService _devicePermissionService;
    private readonly ITaskManager _taskManager;
    private readonly INotificationService _notificationService;
    private readonly IWiiUsbReadinessService _readinessService;
    private readonly IUsbPreparationFileSystem _fileSystem;
    private readonly IWiimmsFormatCommandExecutor _wiimmsExecutor;

    public UsbFormatPreparationService(
        IUsbDeviceProvider usbDeviceProvider,
        IExternalToolManager externalToolManager,
        IDevicePermissionService devicePermissionService,
        ITaskManager taskManager,
        INotificationService notificationService,
        IWiiUsbReadinessService readinessService,
        IUsbPreparationFileSystem fileSystem,
        IWiimmsFormatCommandExecutor wiimmsExecutor)
    {
        _usbDeviceProvider = usbDeviceProvider;
        _externalToolManager = externalToolManager;
        _devicePermissionService = devicePermissionService;
        _taskManager = taskManager;
        _notificationService = notificationService;
        _readinessService = readinessService;
        _fileSystem = fileSystem;
        _wiimmsExecutor = wiimmsExecutor;
    }

    public async Task<UsbFormatPreview> CreatePreviewAsync(
        UsbFormatPreparationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TargetDeviceId);
        var devices = await _usbDeviceProvider.GetDevicesAsync(cancellationToken);
        var target = FindTarget(devices, request);
        var warnings = CreateWarnings(request.Mode);

        if (target is null)
        {
            return CreateBlockedPreview(request, "Dispositivo desconhecido", null, null, warnings, "usbFormat.targetUnknown");
        }

        var (device, partition) = target.Value;
        var blockReason = GetTargetBlockReason(device, partition, request.Mode);
        var toolStatus = request.Mode == UsbPreparationMode.FormatAsWbfs
            ? await _externalToolManager.CheckAsync("wiimms-wwt", cancellationToken)
            : null;
        if (request.Mode == UsbPreparationMode.FormatAsWbfs && !IsToolReady(toolStatus))
        {
            blockReason ??= "usbFormat.wwtUnavailable";
        }

        if (request.Mode == UsbPreparationMode.FormatAsWbfs && !_wiimmsExecutor.HasCataloguedFormatWbfsCommand)
        {
            blockReason ??= "usbFormat.wbfsCommandNotCatalogued";
        }

        var permissionStatus = await CheckPermissionAsync(device, partition, request.Mode, toolStatus?.ExecutablePath, cancellationToken);
        if (IsPermissionBlocked(permissionStatus))
        {
            blockReason ??= "usbFormat.permissionDenied";
        }

        return new UsbFormatPreview(
            device.DeviceId,
            partition?.PartitionId,
            device.DisplayName,
            device.PhysicalPath,
            partition?.DevicePath,
            partition?.MountPath ?? device.MountPath,
            partition?.FileSystem,
            partition?.SizeBytes ?? device.SizeBytes,
            request.Mode,
            request.Mode == UsbPreparationMode.FormatAsWbfs,
            blockReason is not null,
            blockReason,
            request.Mode == UsbPreparationMode.FormatAsWbfs ? WbfsConfirmationPhrase : string.Empty,
            permissionStatus,
            warnings);
    }

    public async Task<UsbFormatPreparationResult> ExecuteAsync(
        UsbFormatExecutionRequest request,
        IProgress<UsbFormatProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.UserConfirmed)
        {
            return Failed(request.Preview, "usbFormat.confirmationRequired");
        }

        if (request.Preview.IsBlocked)
        {
            return Failed(request.Preview, request.Preview.BlockReasonKey ?? "usbFormat.blocked");
        }

        if (request.Preview.IsDestructive &&
            !string.Equals(request.ConfirmationPhrase, WbfsConfirmationPhrase, StringComparison.Ordinal))
        {
            return Failed(request.Preview, "usbFormat.strongConfirmationRequired");
        }

        var taskId = await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Verification, "Preparando dispositivo USB", request.Preview.TargetDisplayName),
            cancellationToken);

        try
        {
            progress?.Report(new UsbFormatProgress(null, UsbFormatWorkflowState.Running, "Preparando dispositivo USB.", IsIndeterminate: true));
            await _taskManager.UpdateProgressAsync(taskId, 0.1, ManagedTaskStatus.Running, cancellationToken);

            var result = request.Preview.Mode == UsbPreparationMode.PrepareFat32WbfsFolder
                ? await PrepareFat32Async(request.Preview, progress, cancellationToken)
                : await FormatWbfsAsync(request.Preview, progress, cancellationToken);

            if (!result.Success)
            {
                await _taskManager.FailAsync(taskId, result.ErrorMessageKey ?? "usbFormat.failed", cancellationToken);
                await PublishAsync(NotificationType.Error, "Falha ao preparar USB", result.ErrorMessageKey ?? request.Preview.TargetDisplayName, taskId, cancellationToken);
                return result;
            }

            await _taskManager.UpdateProgressAsync(taskId, 0.9, ManagedTaskStatus.Running, cancellationToken);
            var diagnosticsPassed = await RunPostDiagnosticsAsync(request.Preview, cancellationToken);
            await _taskManager.CompleteAsync(taskId, cancellationToken);
            await PublishAsync(NotificationType.Success, "Dispositivo USB preparado", request.Preview.TargetDisplayName, taskId, cancellationToken);
            progress?.Report(new UsbFormatProgress(100, UsbFormatWorkflowState.Completed, "Dispositivo pronto para Wii.", IsIndeterminate: false));

            return result with { PostFormatDiagnosticsPassed = diagnosticsPassed };
        }
        catch (OperationCanceledException)
        {
            await _taskManager.CancelAsync(taskId, CancellationToken.None);
            return Failed(request.Preview, "usbFormat.cancelled");
        }
        catch
        {
            await _taskManager.FailAsync(taskId, "usbFormat.failed", CancellationToken.None);
            await PublishAsync(NotificationType.Error, "Falha ao preparar USB", request.Preview.TargetDisplayName, taskId, CancellationToken.None);
            return Failed(request.Preview, "usbFormat.failed");
        }
    }

    private async Task<UsbFormatPreparationResult> PrepareFat32Async(
        UsbFormatPreview preview,
        IProgress<UsbFormatProgress>? progress,
        CancellationToken cancellationToken)
    {
        var mountPath = preview.MountPath;
        if (string.IsNullOrWhiteSpace(mountPath))
        {
            return Failed(preview, "usbFormat.mountPathMissing");
        }

        if (!_fileSystem.CanWriteDirectory(mountPath))
        {
            return Failed(preview, "usbFormat.targetReadOnly");
        }

        var wbfsPath = Path.Combine(mountPath, "wbfs");
        if (!_fileSystem.DirectoryExists(wbfsPath))
        {
            progress?.Report(new UsbFormatProgress(40, UsbFormatWorkflowState.Running, "Criando pasta /wbfs.", IsIndeterminate: false));
            _fileSystem.CreateDirectory(wbfsPath);
        }

        return new UsbFormatPreparationResult(true, preview.Mode, preview.TargetDeviceId, "FAT32", null, PostFormatDiagnosticsPassed: false);
    }

    private async Task<UsbFormatPreparationResult> FormatWbfsAsync(
        UsbFormatPreview preview,
        IProgress<UsbFormatProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (!_wiimmsExecutor.HasCataloguedFormatWbfsCommand)
        {
            return Failed(preview, "usbFormat.wbfsCommandNotCatalogued");
        }

        await _wiimmsExecutor.ExecuteFormatWbfsAsync(preview, progress, cancellationToken);
        var valid = await _wiimmsExecutor.ValidateWbfsAsync(preview, cancellationToken);
        return valid
            ? new UsbFormatPreparationResult(true, preview.Mode, preview.TargetDeviceId, "WBFS", null, PostFormatDiagnosticsPassed: false)
            : Failed(preview, "usbFormat.postValidationFailed");
    }

    private async Task<bool> RunPostDiagnosticsAsync(UsbFormatPreview preview, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(preview.TargetDeviceId, out var driveId))
        {
            return true;
        }

        try
        {
            var result = await _readinessService.CheckAsync(driveId, cancellationToken);
            return result.State != ReadinessState.NotReady;
        }
        catch
        {
            return false;
        }
    }

    private async Task<DevicePermissionStatus?> CheckPermissionAsync(
        UsbDeviceInfo device,
        UsbPartitionInfo? partition,
        UsbPreparationMode mode,
        string? executablePath,
        CancellationToken cancellationToken)
    {
        if (mode == UsbPreparationMode.PrepareFat32WbfsFolder)
        {
            return device.PermissionStatus;
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        return await _devicePermissionService.CheckAsync(
            new DevicePermissionRequest("wwt", executablePath, partition?.DevicePath ?? device.PhysicalPath, PrivilegedDeviceOperation.FormatWbfsPartition),
            cancellationToken);
    }

    private static (UsbDeviceInfo Device, UsbPartitionInfo? Partition)? FindTarget(
        IReadOnlyList<UsbDeviceInfo> devices,
        UsbFormatPreparationRequest request)
    {
        var device = devices.FirstOrDefault(item => string.Equals(item.DeviceId, request.TargetDeviceId, StringComparison.OrdinalIgnoreCase));
        if (device is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.TargetPartitionId))
        {
            return (device, device.Partitions.Count == 1 ? device.Partitions[0] : null);
        }

        var partition = device.Partitions.FirstOrDefault(item => string.Equals(item.PartitionId, request.TargetPartitionId, StringComparison.OrdinalIgnoreCase));
        return partition is null ? null : (device, partition);
    }

    private static string? GetTargetBlockReason(UsbDeviceInfo device, UsbPartitionInfo? partition, UsbPreparationMode mode)
    {
        if (!device.IsUsb || !device.IsRemovable)
        {
            return "usbFormat.systemOrInternalDiskBlocked";
        }

        if (partition is null)
        {
            return "usbFormat.partitionRequired";
        }

        if (IsSystemMount(partition.MountPath) || IsSystemMount(device.MountPath))
        {
            return "usbFormat.systemOrInternalDiskBlocked";
        }

        if (IsApplicationDisk(partition.MountPath ?? device.MountPath))
        {
            return "usbFormat.applicationDiskBlocked";
        }

        if (string.IsNullOrWhiteSpace(partition.DevicePath) && mode == UsbPreparationMode.FormatAsWbfs)
        {
            return "usbFormat.devicePathMissing";
        }

        if (mode == UsbPreparationMode.FormatAsWbfs && IsMountedVolume(partition))
        {
            return "usbFormat.targetMounted";
        }

        if (mode == UsbPreparationMode.PrepareFat32WbfsFolder)
        {
            if (!string.Equals(partition.FileSystem, "FAT32", StringComparison.OrdinalIgnoreCase))
            {
                return "usbFormat.fat32Required";
            }

            if (string.IsNullOrWhiteSpace(partition.MountPath))
            {
                return "usbFormat.mountPathMissing";
            }

            if (!partition.IsWritable)
            {
                return "usbFormat.targetReadOnly";
            }
        }

        if (mode == UsbPreparationMode.FormatAsWbfs && string.IsNullOrWhiteSpace(partition.FileSystem))
        {
            return "usbFormat.unknownRiskBlocked";
        }

        return null;
    }

    private static bool IsMountedVolume(UsbPartitionInfo partition)
    {
        if (string.IsNullOrWhiteSpace(partition.MountPath))
        {
            return false;
        }

        if (string.Equals(partition.MountPath, partition.DevicePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !partition.MountPath.StartsWith("/dev/", StringComparison.OrdinalIgnoreCase)
            && !partition.MountPath.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsToolReady(ExternalToolStatus? status) =>
        status?.State is ExternalToolState.Installed or ExternalToolState.Configured
        && !string.IsNullOrWhiteSpace(status.ExecutablePath);

    private static bool IsPermissionBlocked(DevicePermissionStatus? permissionStatus) =>
        permissionStatus?.State is PrivilegedAccessState.PolicyMissing
            or PrivilegedAccessState.HelperMissing
            or PrivilegedAccessState.PermissionDenied
            or PrivilegedAccessState.Failed
            or PrivilegedAccessState.Unsupported;

    private static bool IsSystemMount(string? mountPath)
    {
        if (string.IsNullOrWhiteSpace(mountPath))
        {
            return false;
        }

        var normalized = Path.GetFullPath(mountPath);
        return normalized == Path.GetPathRoot(normalized)
            || normalized.Equals("/boot", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("/boot/", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsApplicationDisk(string? mountPath)
    {
        if (string.IsNullOrWhiteSpace(mountPath))
        {
            return false;
        }

        var root = Path.GetFullPath(mountPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var appBase = Path.GetFullPath(AppContext.BaseDirectory);
        return appBase.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> CreateWarnings(UsbPreparationMode mode) =>
        mode == UsbPreparationMode.FormatAsWbfs
            ? ["usbFormat.wbfsDestructive", "usbFormat.fat32Recommended"]
            : ["usbFormat.fat32Recommended", "usbFormat.dataPreserved"];

    private static UsbFormatPreview CreateBlockedPreview(
        UsbFormatPreparationRequest request,
        string targetName,
        string? deviceId,
        string? partitionId,
        IReadOnlyList<string> warnings,
        string blockReason) =>
        new(
            deviceId ?? request.TargetDeviceId,
            partitionId ?? request.TargetPartitionId,
            targetName,
            null,
            null,
            null,
            null,
            null,
            request.Mode,
            request.Mode == UsbPreparationMode.FormatAsWbfs,
            IsBlocked: true,
            blockReason,
            request.Mode == UsbPreparationMode.FormatAsWbfs ? WbfsConfirmationPhrase : string.Empty,
            null,
            warnings);

    private static UsbFormatPreparationResult Failed(UsbFormatPreview preview, string errorMessageKey) =>
        new(false, preview.Mode, preview.TargetDeviceId, null, errorMessageKey, PostFormatDiagnosticsPassed: false);

    private Task PublishAsync(NotificationType type, string title, string message, Guid taskId, CancellationToken cancellationToken) =>
        _notificationService.PublishAsync(
            NotificationMessage.Create(type, title, message, isPersistent: type == NotificationType.Error, groupKey: taskId.ToString("N")),
            cancellationToken);
}

public sealed class PhysicalUsbPreparationFileSystem : IUsbPreparationFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public bool CanWriteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        try
        {
            var probe = Path.Combine(path, $".wiistudio-write-test-{Guid.NewGuid():N}");
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class DisabledWiimmsFormatCommandExecutor : IWiimmsFormatCommandExecutor
{
    public bool HasCataloguedFormatWbfsCommand => false;

    public Task ExecuteFormatWbfsAsync(
        UsbFormatPreview preview,
        IProgress<UsbFormatProgress>? progress,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("WBFS format command has not been catalogued and validated yet.");
    }

    public Task<bool> ValidateWbfsAsync(UsbFormatPreview preview, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
