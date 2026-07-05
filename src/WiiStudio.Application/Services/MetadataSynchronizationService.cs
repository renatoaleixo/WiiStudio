using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class MetadataSynchronizationService : IMetadataSynchronizationService
{
    private readonly IMetadataCacheService _metadataCacheService;
    private readonly IReadOnlyList<IGameMetadataProvider> _metadataProviders;

    public MetadataSynchronizationService(
        IMetadataCacheService metadataCacheService,
        IEnumerable<IGameMetadataProvider> metadataProviders)
    {
        _metadataCacheService = metadataCacheService;
        _metadataProviders = metadataProviders.ToArray();
    }

    public async Task<MetadataSyncPreview> AnalyzeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        var items = new List<MetadataSyncItem>();
        foreach (var gameId in gameIds)
        {
            var normalizedGameId = GameIdValidation.Normalize(gameId);
            var cachedMetadata = await _metadataCacheService.GetAsync(normalizedGameId, cancellationToken);
            items.Add(cachedMetadata is null
                ? new MetadataSyncItem(normalizedGameId, MetadataSyncStatus.Missing, MetadataSyncSource.None, null, "Metadata is not cached.")
                : new MetadataSyncItem(normalizedGameId, MetadataSyncStatus.Cached, MetadataSyncSource.Cache, cachedMetadata, "Metadata is already cached."));
        }

        return new MetadataSyncPreview(items);
    }

    public async Task<MetadataSyncResult> SynchronizeAsync(MetadataSyncRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var items = new List<MetadataSyncItem>();
        foreach (var gameId in request.GameIds)
        {
            var normalizedGameId = GameIdValidation.Normalize(gameId);
            items.Add(await SynchronizeItemAsync(normalizedGameId, request.ForceRefresh, cancellationToken));
        }

        return new MetadataSyncResult(items, DateTimeOffset.UtcNow);
    }

    private async Task<MetadataSyncItem> SynchronizeItemAsync(
        string gameId,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        if (!forceRefresh)
        {
            var cachedMetadata = await _metadataCacheService.GetAsync(gameId, cancellationToken);
            if (cachedMetadata is not null)
            {
                return new MetadataSyncItem(gameId, MetadataSyncStatus.Cached, MetadataSyncSource.Cache, cachedMetadata, "Metadata reused from cache.");
            }
        }

        var provider = await FindAvailableProviderAsync(cancellationToken);
        if (provider is null)
        {
            return new MetadataSyncItem(gameId, MetadataSyncStatus.ProviderUnavailable, MetadataSyncSource.None, null, "No metadata provider is available.");
        }

        try
        {
            var metadata = await provider.GetByGameIdAsync(gameId, cancellationToken);
            if (metadata is null)
            {
                return new MetadataSyncItem(gameId, MetadataSyncStatus.Missing, MetadataSyncSource.Provider, null, "Provider did not return metadata for this game.");
            }

            metadata = metadata with { GameId = GameIdValidation.Normalize(metadata.GameId) };
            await _metadataCacheService.SaveAsync(metadata, cancellationToken);

            return new MetadataSyncItem(gameId, MetadataSyncStatus.Updated, MetadataSyncSource.Provider, metadata, "Metadata synchronized from provider.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new MetadataSyncItem(gameId, MetadataSyncStatus.Failed, MetadataSyncSource.Provider, null, exception.Message);
        }
    }

    private async Task<IGameMetadataProvider?> FindAvailableProviderAsync(CancellationToken cancellationToken)
    {
        foreach (var provider in _metadataProviders)
        {
            if (await provider.IsAvailableAsync(cancellationToken))
            {
                return provider;
            }
        }

        return null;
    }
}
