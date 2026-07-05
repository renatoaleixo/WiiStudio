using Microsoft.Extensions.Logging;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WitService : IWitService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(2);

    private readonly IProcessRunner _processRunner;
    private readonly ILogger<WitService> _logger;

    public WitService(IProcessRunner processRunner, ILogger<WitService> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);
        cancellationToken.ThrowIfCancellationRequested();

        var request = new ProcessRequest(
            "wit",
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
}
