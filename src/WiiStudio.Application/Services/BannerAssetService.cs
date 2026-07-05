using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class BannerAssetService : IBannerAssetService
{
    private static readonly CoverAssetKind[] RequiredAssetKinds =
    [
        CoverAssetKind.Banner,
        CoverAssetKind.DiscArt
    ];

    private readonly IAssetManager _assetManager;

    public BannerAssetService(IAssetManager assetManager)
    {
        _assetManager = assetManager;
    }

    public async Task<AssetDownloadResult> DownloadAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        cancellationToken.ThrowIfCancellationRequested();

        var items = new List<AssetDownloadItem>();
        foreach (var gameId in gameIds)
        {
            var normalizedGameId = GameIdValidation.Normalize(gameId);
            foreach (var kind in RequiredAssetKinds)
            {
                items.Add(await DownloadItemAsync(normalizedGameId, kind, forceRefresh: false, cancellationToken));
            }
        }

        return new AssetDownloadResult(items, DateTimeOffset.UtcNow);
    }

    public async Task<bool> HasAssetsAsync(string gameId, CancellationToken cancellationToken)
    {
        var normalizedGameId = GameIdValidation.Normalize(gameId);
        foreach (var kind in RequiredAssetKinds)
        {
            var asset = await _assetManager.GetAsync(
                new AssetRequest(new AssetKey(normalizedGameId, kind), AllowProviderFallback: false),
                cancellationToken);
            if (asset is null)
            {
                return false;
            }
        }

        return true;
    }

    public async Task RefreshAsync(string gameId, CancellationToken cancellationToken)
    {
        var normalizedGameId = GameIdValidation.Normalize(gameId);
        foreach (var kind in RequiredAssetKinds)
        {
            await DownloadItemAsync(normalizedGameId, kind, forceRefresh: true, cancellationToken);
        }
    }

    private async Task<AssetDownloadItem> DownloadItemAsync(
        string gameId,
        CoverAssetKind kind,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        try
        {
            var asset = await _assetManager.GetAsync(
                new AssetRequest(new AssetKey(gameId, kind), ForceRefresh: forceRefresh),
                cancellationToken);
            if (asset is null)
            {
                return new AssetDownloadItem(gameId, kind, AssetDownloadStatus.Missing, CoverAssetSource.Missing, null, "Asset is not available.");
            }

            var status = asset.Source == CoverAssetSource.Provider ? AssetDownloadStatus.Downloaded : AssetDownloadStatus.Cached;
            var message = asset.Source == CoverAssetSource.Provider ? "Asset downloaded from provider." : "Asset reused from cache.";

            return new AssetDownloadItem(gameId, kind, status, asset.Source, asset, message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new AssetDownloadItem(gameId, kind, AssetDownloadStatus.Failed, CoverAssetSource.Missing, null, exception.Message);
        }
    }
}
