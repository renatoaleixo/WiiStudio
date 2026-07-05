using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ICoverCache
{
    Task<string?> GetAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken);

    Task<string> SaveAsync(string gameId, CoverAssetKind kind, Stream content, CancellationToken cancellationToken);

    Task<bool> RemoveAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken);

    Task RebuildIndexAsync(CancellationToken cancellationToken);

    Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken);
}
