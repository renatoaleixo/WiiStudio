using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class CoverDownloadService : ICoverDownloadService
{
    private static readonly CoverAssetKind[] DefaultAssetKinds =
    [
        CoverAssetKind.FrontCover,
        CoverAssetKind.BackCover,
        CoverAssetKind.DiscArt,
        CoverAssetKind.Banner,
        CoverAssetKind.Icon
    ];

    private readonly ICoverCache _coverCache;
    private readonly ICoverProvider _coverProvider;

    public CoverDownloadService(ICoverCache coverCache, ICoverProvider coverProvider)
    {
        _coverCache = coverCache;
        _coverProvider = coverProvider;
    }

    public async Task<CoverDownloadPreview> AnalyzeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        var request = new CoverDownloadRequest(gameIds);
        var items = await AnalyzeItemsAsync(request, cancellationToken);

        return new CoverDownloadPreview(items);
    }

    public async Task<CoverDownloadResult> DownloadAsync(CoverDownloadRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var items = new List<CoverDownloadItem>();
        foreach (var gameId in request.GameIds)
        {
            var normalizedGameId = GameIdValidation.Normalize(gameId);
            foreach (var kind in GetAssetKinds(request.AssetKinds))
            {
                cancellationToken.ThrowIfCancellationRequested();
                items.Add(await DownloadItemAsync(normalizedGameId, kind, request.ForceRefresh, cancellationToken));
            }
        }

        return new CoverDownloadResult(items, DateTimeOffset.UtcNow);
    }

    private async Task<IReadOnlyList<CoverDownloadItem>> AnalyzeItemsAsync(CoverDownloadRequest request, CancellationToken cancellationToken)
    {
        var items = new List<CoverDownloadItem>();
        foreach (var gameId in request.GameIds)
        {
            var normalizedGameId = GameIdValidation.Normalize(gameId);
            foreach (var kind in GetAssetKinds(request.AssetKinds))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var cachedPath = await _coverCache.GetAsync(normalizedGameId, kind, cancellationToken);
                items.Add(cachedPath is null
                    ? new CoverDownloadItem(normalizedGameId, kind, CoverAssetSource.Provider, null, "Asset will be downloaded from provider.")
                    : new CoverDownloadItem(normalizedGameId, kind, CoverAssetSource.Cache, cachedPath, "Asset is already cached."));
            }
        }

        return items;
    }

    private async Task<CoverDownloadItem> DownloadItemAsync(
        string gameId,
        CoverAssetKind kind,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        if (!forceRefresh)
        {
            var cachedPath = await _coverCache.GetAsync(gameId, kind, cancellationToken);
            if (cachedPath is not null)
            {
                return new CoverDownloadItem(gameId, kind, CoverAssetSource.Cache, cachedPath, "Asset reused from cache.");
            }
        }

        await using var content = await _coverProvider.DownloadAsync(gameId, kind, cancellationToken);
        if (content is null)
        {
            return new CoverDownloadItem(gameId, kind, CoverAssetSource.Missing, null, "Provider did not return this asset.");
        }

        var localPath = await _coverCache.SaveAsync(gameId, kind, content, cancellationToken);

        return new CoverDownloadItem(gameId, kind, CoverAssetSource.Provider, localPath, "Asset downloaded from provider.");
    }

    private static IReadOnlyList<CoverAssetKind> GetAssetKinds(IReadOnlyList<CoverAssetKind>? assetKinds)
    {
        return assetKinds is { Count: > 0 } ? assetKinds : DefaultAssetKinds;
    }
}
