using Microsoft.Extensions.Logging;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WwtService : IWwtService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromHours(2);

    private readonly IProcessRunner _processRunner;
    private readonly IWiimmsToolLocator _toolLocator;
    private readonly ILogger<WwtService> _logger;
    private readonly IPrivilegedAccessService? _privilegedAccessService;
    private readonly IWiimmsProgressParser? _progressParser;
    private readonly ITaskManager? _taskManager;

    public WwtService(
        IProcessRunner processRunner,
        IWiimmsToolLocator toolLocator,
        ILogger<WwtService> logger,
        IPrivilegedAccessService? privilegedAccessService = null,
        IWiimmsProgressParser? progressParser = null,
        ITaskManager? taskManager = null)
    {
        _processRunner = processRunner;
        _toolLocator = toolLocator;
        _logger = logger;
        _privilegedAccessService = privilegedAccessService;
        _progressParser = progressParser;
        _taskManager = taskManager;
    }

    public async Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);
        cancellationToken.ThrowIfCancellationRequested();

        var wwtPath = await _toolLocator.FindWwtAsync(cancellationToken);
        var request = new ProcessRequest(
            wwtPath,
            [
                "list",
                "--part",
                drive.MountPoint
            ],
            TimeSpan.FromMinutes(5));

        var result = await RunWithElevationFallbackAsync(request, "wwt list", cancellationToken);
        var games = ParseListOutput(result.StandardOutput, drive);
        _logger.LogInformation("Parsed {GameCount} game(s) from wwt list", games.Count);

        return games;
    }

    public async Task AddGameAsync(
        UsbDrive drive,
        string sourcePath,
        CancellationToken cancellationToken,
        IProgress<WiimmsOperationProgress>? progress = null,
        bool publishTask = true)
    {
        ArgumentNullException.ThrowIfNull(drive);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        cancellationToken.ThrowIfCancellationRequested();

        var wwtPath = await _toolLocator.FindWwtAsync(cancellationToken);
        var taskId = publishTask
            ? await EnqueueWiimmsTaskAsync(
                "Adicionando jogo ao USB",
                $"{Path.GetFileName(sourcePath)} -> {drive.MountPoint}",
                cancellationToken)
            : null;
        var progressCallback = CreateProgressCallback(taskId, progress, cancellationToken);
        var request = new ProcessRequest(
            wwtPath,
            [
                "add",
                "--part",
                drive.MountPoint,
                "--source",
                sourcePath,
                "--update",
                "--progress"
            ],
            DefaultTimeout,
            progressCallback,
            progressCallback);

        try
        {
            await UpdateWiimmsTaskAsync(taskId, 0.01, ManagedTaskStatus.Running, cancellationToken);
            await RunOrThrowAsync(request, "wwt add", cancellationToken);
            await CompleteWiimmsTaskAsync(taskId, cancellationToken);
            _logger.LogInformation("Added game source {SourcePath} to USB drive {DriveId}", sourcePath, drive.Id);
        }
        catch (OperationCanceledException)
        {
            await CancelWiimmsTaskAsync(taskId);
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await FailWiimmsTaskAsync(taskId, exception.Message);
            throw;
        }
    }

    public async Task RemoveGameAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);
        ArgumentException.ThrowIfNullOrWhiteSpace(gameId);
        cancellationToken.ThrowIfCancellationRequested();

        var wwtPath = await _toolLocator.FindWwtAsync(cancellationToken);
        var request = new ProcessRequest(
            wwtPath,
            [
                "remove",
                "--part",
                drive.MountPoint,
                gameId
            ],
            DefaultTimeout);

        await RunOrThrowAsync(request, "wwt remove", cancellationToken);
        _logger.LogInformation("Removed game {GameId} from USB drive {DriveId}", gameId, drive.Id);
    }

    private async Task RunOrThrowAsync(ProcessRequest request, string operationName, CancellationToken cancellationToken)
    {
        _ = await RunWithElevationFallbackAsync(request, operationName, cancellationToken);
    }

    private async Task<ProcessResult> RunWithElevationFallbackAsync(
        ProcessRequest request,
        string operationName,
        CancellationToken cancellationToken)
    {
        var result = await RunProcessOrTimeoutResultAsync(request, cancellationToken);
        if (!result.IsSuccess && HasPermissionDenied(result))
        {
            var privilegedResult = await RunPrivilegedFallbackAsync(request, operationName, cancellationToken);
            if (privilegedResult is not null)
            {
                result = privilegedResult;
            }
        }

        if (!result.IsSuccess)
        {
            throw new WiimmsCommandException(
                $"{operationName} failed with exit code {result.ExitCode}.",
                result.ExitCode,
                result.StandardError);
        }

        return result;
    }

    private async Task<ProcessResult?> RunPrivilegedFallbackAsync(
        ProcessRequest request,
        string operationName,
        CancellationToken cancellationToken)
    {
        if (_privilegedAccessService is not null)
        {
            var operation = MapOperation(operationName);
            var devicePath = ExtractDevicePath(request.Arguments);
            _logger.LogInformation(
                "Retrying {OperationName} through privileged access service because raw device access was denied.",
                operationName);
            var result = await _privilegedAccessService.RunKnownToolAsync(
                new PrivilegedToolRequest(
                    "wwt",
                    request.ExecutablePath,
                    request.Arguments,
                    request.Timeout,
                    operation,
                    DevicePath: devicePath),
                cancellationToken);

            return new ProcessResult(result.ExitCode, result.StandardOutput, result.StandardError, result.Duration);
        }

        if (!TryCreatePkexecRequest(request, out var elevatedRequest))
        {
            return null;
        }

        _logger.LogInformation("Retrying {OperationName} through pkexec because raw device access was denied.", operationName);
        return await RunProcessOrTimeoutResultAsync(elevatedRequest, cancellationToken);
    }

    private async Task<ProcessResult> RunProcessOrTimeoutResultAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await _processRunner.RunAsync(request, cancellationToken);
        }
        catch (TimeoutException exception)
        {
            return new ProcessResult(124, string.Empty, exception.Message, request.Timeout);
        }
    }

    private Action<string> CreateProgressCallback(
        Guid? taskId,
        IProgress<WiimmsOperationProgress>? progressReporter,
        CancellationToken cancellationToken)
    {
        return line =>
        {
            if (_progressParser?.TryParse(line, out var progress) != true)
            {
                return;
            }

            TrackWiimmsOutput(progress);
            progressReporter?.Report(progress);
            if (taskId is null)
            {
                return;
            }

            var value = progress.Percentage is null
                ? 0.01
                : Math.Clamp(progress.Percentage.Value / 100d, 0.01, 0.99);

            try
            {
                _taskManager!
                    .UpdateProgressAsync(taskId.Value, value, ManagedTaskStatus.Running, cancellationToken)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Failed to update Wiimms task progress.");
            }
        };
    }

    private void TrackWiimmsOutput(WiimmsOperationProgress progress)
    {
        var progressText = progress.Percentage is null
            ? "indeterminate"
            : $"{progress.Percentage:0.#}%";
        _logger.LogInformation(
            "Wiimms progress {Progress}: {Operation}",
            progressText,
            progress.CurrentOperation ?? progress.CurrentFile ?? "running");
    }

    private async Task<Guid?> EnqueueWiimmsTaskAsync(
        string title,
        string description,
        CancellationToken cancellationToken)
    {
        if (_taskManager is null)
        {
            return null;
        }

        return await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Import, title, description, ManagedTaskPriority.High),
            cancellationToken);
    }

    private async Task UpdateWiimmsTaskAsync(
        Guid? taskId,
        double progress,
        ManagedTaskStatus status,
        CancellationToken cancellationToken)
    {
        if (_taskManager is null || taskId is null)
        {
            return;
        }

        await _taskManager.UpdateProgressAsync(taskId.Value, progress, status, cancellationToken);
    }

    private async Task CompleteWiimmsTaskAsync(Guid? taskId, CancellationToken cancellationToken)
    {
        if (_taskManager is null || taskId is null)
        {
            return;
        }

        await _taskManager.CompleteAsync(taskId.Value, cancellationToken);
    }

    private async Task CancelWiimmsTaskAsync(Guid? taskId)
    {
        if (_taskManager is null || taskId is null)
        {
            return;
        }

        await _taskManager.CancelAsync(taskId.Value, CancellationToken.None);
    }

    private async Task FailWiimmsTaskAsync(Guid? taskId, string errorMessage)
    {
        if (_taskManager is null || taskId is null)
        {
            return;
        }

        await _taskManager.FailAsync(taskId.Value, errorMessage, CancellationToken.None);
    }

    private static bool HasPermissionDenied(ProcessResult result)
    {
        return result.StandardError.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)
            || result.StandardOutput.Contains("Permission denied", StringComparison.OrdinalIgnoreCase);
    }

    private static PrivilegedDeviceOperation MapOperation(string operationName)
    {
        return operationName switch
        {
            "wwt list" => PrivilegedDeviceOperation.ListWbfsGames,
            "wwt add" => PrivilegedDeviceOperation.AddWbfsGame,
            "wwt remove" => PrivilegedDeviceOperation.RemoveWbfsGame,
            _ => PrivilegedDeviceOperation.InspectDevice
        };
    }

    private static string? ExtractDevicePath(IReadOnlyList<string> arguments)
    {
        for (var index = 0; index < arguments.Count - 1; index++)
        {
            if (string.Equals(arguments[index], "--part", StringComparison.OrdinalIgnoreCase))
            {
                return arguments[index + 1];
            }
        }

        return arguments.FirstOrDefault(argument => argument.StartsWith("/dev/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryCreatePkexecRequest(ProcessRequest request, out ProcessRequest elevatedRequest)
    {
        elevatedRequest = default!;
        var pkexecPath = FindExecutable("pkexec");
        if (pkexecPath is null)
        {
            return false;
        }

        elevatedRequest = new ProcessRequest(
            pkexecPath,
            [request.ExecutablePath, .. request.Arguments],
            request.Timeout,
            request.OnStandardOutputLine,
            request.OnStandardErrorLine);
        return true;
    }

    private static string? FindExecutable(string executableName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory, executableName))
            .FirstOrDefault(File.Exists);
    }

    private static IReadOnlyList<Game> ParseListOutput(string output, UsbDrive drive)
    {
        var games = new List<Game>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!TryParseGameLine(line, drive, out var game))
            {
                continue;
            }

            games.Add(game);
        }

        return games;
    }

    private static bool TryParseGameLine(string line, UsbDrive drive, out Game game)
    {
        game = default!;
        if (line.Length < 8
            || line.StartsWith('-')
            || line.StartsWith("ID", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("Total:", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("wwt:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var id = line[..6].Trim();
        if (id.Length != 6 || id.Any(character => !char.IsLetterOrDigit(character)))
        {
            return false;
        }

        var title = line[6..].Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        try
        {
            game = new Game(
                new GameId(id),
                title,
                Region.Unknown,
                DiscType.Wii,
                FileFormat.Wbfs,
                $"{drive.MountPoint}:{id}",
                0,
                DateTimeOffset.UtcNow);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
