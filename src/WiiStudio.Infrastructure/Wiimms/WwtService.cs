using Microsoft.Extensions.Logging;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WwtService : IWwtService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromHours(2);

    private readonly IProcessRunner _processRunner;
    private readonly IWiimmsToolLocator _toolLocator;
    private readonly ILogger<WwtService> _logger;

    public WwtService(IProcessRunner processRunner, IWiimmsToolLocator toolLocator, ILogger<WwtService> logger)
    {
        _processRunner = processRunner;
        _toolLocator = toolLocator;
        _logger = logger;
    }

    public async Task AddGameAsync(UsbDrive drive, string sourcePath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        cancellationToken.ThrowIfCancellationRequested();

        var wwtPath = await _toolLocator.FindWwtAsync(cancellationToken);
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
            DefaultTimeout);

        await RunOrThrowAsync(request, "wwt add", cancellationToken);
        _logger.LogInformation("Added game source {SourcePath} to USB drive {DriveId}", sourcePath, drive.Id);
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
        var result = await _processRunner.RunAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new WiimmsCommandException(
                $"{operationName} failed with exit code {result.ExitCode}.",
                result.ExitCode,
                result.StandardError);
        }
    }
}
