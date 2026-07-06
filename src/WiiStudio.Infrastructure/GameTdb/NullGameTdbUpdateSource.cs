using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.GameTdb;

public sealed class NullGameTdbUpdateSource : IGameTdbUpdateSource
{
    public Task<GameTdbUpdateInfo?> GetLatestAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<GameTdbUpdateInfo?>(null);
    }

    public Task<GameTdbUpdatePackage?> DownloadAsync(
        GameTdbUpdateInfo update,
        GameTdbUpdateOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<GameTdbUpdatePackage?>(null);
    }
}
