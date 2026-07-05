using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IAssetManager
{
    Task<AssetInfo?> GetAsync(AssetRequest request, CancellationToken cancellationToken);

    Task SaveAsync(AssetSaveRequest request, CancellationToken cancellationToken);

    Task<bool> RemoveAsync(AssetKey key, CancellationToken cancellationToken);

    Task RebuildIndexAsync(CancellationToken cancellationToken);

    Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken);
}
