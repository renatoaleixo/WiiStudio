using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IBannerAssetService
{
    Task<AssetDownloadResult> DownloadAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);

    Task<bool> HasAssetsAsync(string gameId, CancellationToken cancellationToken);

    Task RefreshAsync(string gameId, CancellationToken cancellationToken);
}
