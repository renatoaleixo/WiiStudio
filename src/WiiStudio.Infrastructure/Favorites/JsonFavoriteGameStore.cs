using System.Text.Json;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Favorites;

public sealed class JsonFavoriteGameStore : IFavoriteGameStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly FavoriteGameStoreOptions _options;

    public JsonFavoriteGameStore(FavoriteGameStoreOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlySet<string>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        await using var stream = File.OpenRead(_options.FilePath);
        var gameIds = await JsonSerializer.DeserializeAsync<string[]>(stream, JsonOptions, cancellationToken);

        return new HashSet<string>(gameIds ?? [], StringComparer.OrdinalIgnoreCase);
    }

    public async Task SaveAsync(IReadOnlySet<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var orderedGameIds = gameIds
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await using var stream = File.Create(_options.FilePath);
        await JsonSerializer.SerializeAsync(stream, orderedGameIds, JsonOptions, cancellationToken);
    }
}
