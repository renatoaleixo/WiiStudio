using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class AssetManager : IAssetManager
{
    private readonly ICoverCache _coverCache;
    private readonly ICoverProvider _coverProvider;
    private readonly IGameAssetRepository? _gameAssetRepository;

    public AssetManager(ICoverCache coverCache, ICoverProvider coverProvider, IGameAssetRepository? gameAssetRepository = null)
    {
        _coverCache = coverCache;
        _coverProvider = coverProvider;
        _gameAssetRepository = gameAssetRepository;
    }

    public async Task<AssetInfo?> GetAsync(AssetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var key = NormalizeKey(request.Key);
        if (!request.ForceRefresh)
        {
            var cachedInfo = await GetCachedAssetAsync(key, cancellationToken);
            if (cachedInfo is not null)
            {
                return cachedInfo;
            }
        }

        if (!request.AllowProviderFallback)
        {
            return null;
        }

        await using var content = await _coverProvider.DownloadAsync(key.GameId, key.Kind, cancellationToken);
        if (content is null)
        {
            return null;
        }

        var localPath = await _coverCache.SaveAsync(key.GameId, key.Kind, content, cancellationToken);
        var info = CreateInfo(key, localPath, CoverAssetSource.Provider);
        if (_gameAssetRepository is not null)
        {
            await _gameAssetRepository.UpsertAsync(info, cancellationToken);
        }

        return info;
    }

    public async Task SaveAsync(AssetSaveRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);
        cancellationToken.ThrowIfCancellationRequested();

        var key = NormalizeKey(request.Key);
        var localPath = await _coverCache.SaveAsync(key.GameId, key.Kind, request.Content, cancellationToken);
        if (_gameAssetRepository is not null)
        {
            await _gameAssetRepository.UpsertAsync(CreateInfo(key, localPath, CoverAssetSource.Provider), cancellationToken);
        }
    }

    public Task<bool> RemoveAsync(AssetKey key, CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizeKey(key);

        return RemoveCoreAsync(normalizedKey, cancellationToken);
    }

    public Task RebuildIndexAsync(CancellationToken cancellationToken)
    {
        return _coverCache.RebuildIndexAsync(cancellationToken);
    }

    public Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        return _coverCache.GetStatisticsAsync(cancellationToken);
    }

    private async Task<AssetInfo?> GetCachedAssetAsync(AssetKey key, CancellationToken cancellationToken)
    {
        var localPath = await _coverCache.GetAsync(key.GameId, key.Kind, cancellationToken);

        return localPath is null ? null : CreateInfo(key, localPath, CoverAssetSource.Cache);
    }

    private async Task<bool> RemoveCoreAsync(AssetKey key, CancellationToken cancellationToken)
    {
        var removed = await _coverCache.RemoveAsync(key.GameId, key.Kind, cancellationToken);
        if (removed && _gameAssetRepository is not null)
        {
            await _gameAssetRepository.DeleteAsync(key, cancellationToken);
        }

        return removed;
    }

    private static AssetInfo CreateInfo(AssetKey key, string localPath, CoverAssetSource source)
    {
        var fileInfo = new FileInfo(localPath);
        var sizeInBytes = fileInfo.Exists ? fileInfo.Length : 0;
        var lastModifiedAt = fileInfo.Exists
            ? new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero)
            : (DateTimeOffset?)null;

        return new AssetInfo(key, localPath, sizeInBytes, lastModifiedAt, source);
    }

    private static AssetKey NormalizeKey(AssetKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key with { GameId = GameIdValidation.Normalize(key.GameId) };
    }
}
