using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.GameTdb;

public sealed class JsonGameTdbUpdateStateStore : IGameTdbUpdateStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly GameTdbUpdateStateStoreOptions _options;

    public JsonGameTdbUpdateStateStore(GameTdbUpdateStateStoreOptions options)
    {
        _options = options;
    }

    public async Task<GameTdbUpdateState> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return new GameTdbUpdateState();
        }

        await using var stream = File.OpenRead(_options.FilePath);
        var state = await JsonSerializer.DeserializeAsync<GameTdbUpdateState>(stream, JsonOptions, cancellationToken);

        return state ?? new GameTdbUpdateState();
    }

    public async Task SaveAsync(GameTdbUpdateState state, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_options.FilePath);
        await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var state = await LoadAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(state.PreviousVersion))
        {
            return;
        }

        await SaveAsync(state with
        {
            InstalledVersion = state.PreviousVersion,
            PreviousVersion = state.InstalledVersion,
            LastUpdatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}
