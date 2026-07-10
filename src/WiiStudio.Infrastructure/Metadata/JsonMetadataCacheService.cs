using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Infrastructure.Metadata;

public sealed class JsonMetadataCacheService : IMetadataCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly MetadataCacheOptions _options;
    private readonly IGameMetadataRepository? _gameMetadataRepository;

    public JsonMetadataCacheService(MetadataCacheOptions options, IGameMetadataRepository? gameMetadataRepository = null)
    {
        _options = options;
        _gameMetadataRepository = gameMetadataRepository;
    }

    public async Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken)
    {
        var normalizedGameId = GameIdValidation.Normalize(gameId);
        var entries = await LoadEntriesAsync(cancellationToken);
        if (entries.TryGetValue(normalizedGameId, out var entry))
        {
            return entry.Metadata;
        }

        return _gameMetadataRepository is null
            ? null
            : await _gameMetadataRepository.GetAsync(normalizedGameId, cancellationToken);
    }

    public async Task SaveAsync(GameMetadataDto metadata, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedMetadata = metadata with
        {
            GameId = GameIdValidation.Normalize(metadata.GameId),
            UpdatedAt = metadata.UpdatedAt ?? DateTimeOffset.UtcNow
        };
        var entries = await LoadEntriesAsync(cancellationToken);
        entries[normalizedMetadata.GameId] = new MetadataCacheEntry(normalizedMetadata, DateTimeOffset.UtcNow);

        await SaveEntriesAsync(entries, cancellationToken);
        if (_gameMetadataRepository is not null)
        {
            await _gameMetadataRepository.UpsertAsync(normalizedMetadata, cancellationToken);
        }
    }

    public async Task InvalidateAsync(string gameId, CancellationToken cancellationToken)
    {
        var normalizedGameId = GameIdValidation.Normalize(gameId);
        var entries = await LoadEntriesAsync(cancellationToken);
        if (entries.Remove(normalizedGameId))
        {
            await SaveEntriesAsync(entries, cancellationToken);
        }

        if (_gameMetadataRepository is not null)
        {
            await _gameMetadataRepository.DeleteAsync(normalizedGameId, cancellationToken);
        }
    }

    public async Task<MetadataCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var entries = await LoadEntriesAsync(cancellationToken);
        var sizeInBytes = File.Exists(_options.FilePath) ? new FileInfo(_options.FilePath).Length : 0;
        DateTimeOffset? lastUpdatedAt = entries.Count == 0 ? null : entries.Values.Max(entry => entry.CachedAt);

        return new MetadataCacheStatistics(entries.Count, sizeInBytes, lastUpdatedAt);
    }

    public async Task CleanupAsync(MetadataCleanupOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (!options.RemoveExpiredOnly)
        {
            await SaveEntriesAsync(new Dictionary<string, MetadataCacheEntry>(StringComparer.OrdinalIgnoreCase), cancellationToken);
            return;
        }

        var expiration = options.Expiration ?? _options.Expiration;
        var cutoff = DateTimeOffset.UtcNow.Subtract(expiration);
        var entries = await LoadEntriesAsync(cancellationToken);
        var activeEntries = entries
            .Where(entry => entry.Value.CachedAt >= cutoff)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);

        if (activeEntries.Count != entries.Count)
        {
            await SaveEntriesAsync(activeEntries, cancellationToken);
        }
    }

    private async Task<Dictionary<string, MetadataCacheEntry>> LoadEntriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return new Dictionary<string, MetadataCacheEntry>(StringComparer.OrdinalIgnoreCase);
        }

        MetadataCacheEntry[]? entries;
        try
        {
            await using var stream = File.OpenRead(_options.FilePath);
            entries = await JsonSerializer.DeserializeAsync<MetadataCacheEntry[]>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return new Dictionary<string, MetadataCacheEntry>(StringComparer.OrdinalIgnoreCase);
        }

        return (entries ?? [])
            .Where(entry => GameIdValidation.IsValid(entry.Metadata.GameId))
            .GroupBy(entry => GameIdValidation.Normalize(entry.Metadata.GameId), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
    }

    private async Task SaveEntriesAsync(
        IReadOnlyDictionary<string, MetadataCacheEntry> entries,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var orderedEntries = entries
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => entry.Value)
            .ToArray();

        await using var stream = File.Create(_options.FilePath);
        await JsonSerializer.SerializeAsync(stream, orderedEntries, JsonOptions, cancellationToken);
    }

    private sealed record MetadataCacheEntry(GameMetadataDto Metadata, DateTimeOffset CachedAt);
}
