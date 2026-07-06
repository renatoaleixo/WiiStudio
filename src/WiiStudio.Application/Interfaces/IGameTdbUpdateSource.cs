using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameTdbUpdateSource
{
    Task<GameTdbUpdateInfo?> GetLatestAsync(CancellationToken cancellationToken);

    Task<GameTdbUpdatePackage?> DownloadAsync(
        GameTdbUpdateInfo update,
        GameTdbUpdateOptions options,
        CancellationToken cancellationToken);
}
