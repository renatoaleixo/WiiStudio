using Microsoft.Extensions.Logging;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WitService : IWitService
{
    private static readonly TimeSpan VersionTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(2);

    private readonly IProcessRunner _processRunner;
    private readonly IWiimmsToolLocator _toolLocator;
    private readonly ILogger<WitService> _logger;

    public WitService(IProcessRunner processRunner, IWiimmsToolLocator toolLocator, ILogger<WitService> logger)
    {
        _processRunner = processRunner;
        _toolLocator = toolLocator;
        _logger = logger;
    }

    public async Task<Version> GetVersionAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var witPath = await _toolLocator.FindWitAsync(cancellationToken);
        var request = new ProcessRequest(witPath, ["--version"], VersionTimeout);
        var result = await _processRunner.RunAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new WiimmsCommandException(
                $"wit --version failed with exit code {result.ExitCode}.",
                result.ExitCode,
                result.StandardError);
        }

        var version = WiimmsVersionParser.Parse(result.StandardOutput);
        _logger.LogInformation("Detected wit version {WitVersion}", version);

        return version;
    }

    public async Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);
        cancellationToken.ThrowIfCancellationRequested();

        var witPath = await _toolLocator.FindWitAsync(cancellationToken);
        var request = new ProcessRequest(
            witPath,
            [
                "list",
                drive.MountPoint,
                "--sections",
                "ID,TITLE,REGION,FORMAT,SIZE,FILE",
                "--no-header"
            ],
            DefaultTimeout);

        var result = await _processRunner.RunAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new WiimmsCommandException(
                $"wit list failed with exit code {result.ExitCode}.",
                result.ExitCode,
                result.StandardError);
        }

        var games = WitGameListParser.Parse(result.StandardOutput);
        _logger.LogInformation("Parsed {GameCount} game(s) from wit list", games.Count);

        return games;
    }

    public async Task VerifyAsync(string sourcePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        cancellationToken.ThrowIfCancellationRequested();

        var witPath = await _toolLocator.FindWitAsync(cancellationToken);
        var request = new ProcessRequest(
            witPath,
            [
                "verify",
                sourcePath,
                "--test"
            ],
            DefaultTimeout);

        var result = await _processRunner.RunAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new WiimmsCommandException(
                $"wit verify failed with exit code {result.ExitCode}.",
                result.ExitCode,
                result.StandardError);
        }

        _logger.LogInformation("Verified game source {SourcePath} with wit", sourcePath);
    }

    public async Task CopyAsync(string sourcePath, string destinationPath, FileFormat targetFormat, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        cancellationToken.ThrowIfCancellationRequested();

        var formatArgument = targetFormat switch
        {
            FileFormat.Iso => "--iso",
            FileFormat.Wbfs => "--wbfs",
            _ => throw new ArgumentOutOfRangeException(nameof(targetFormat), targetFormat, "Unsupported export format.")
        };
        var witPath = await _toolLocator.FindWitAsync(cancellationToken);
        var request = new ProcessRequest(
            witPath,
            [
                "copy",
                formatArgument,
                sourcePath,
                destinationPath
            ],
            DefaultTimeout);

        var result = await _processRunner.RunAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new WiimmsCommandException(
                $"wit copy failed with exit code {result.ExitCode}.",
                result.ExitCode,
                result.StandardError);
        }

        _logger.LogInformation("Copied game source {SourcePath} to {DestinationPath} with wit", sourcePath, destinationPath);
    }
}
