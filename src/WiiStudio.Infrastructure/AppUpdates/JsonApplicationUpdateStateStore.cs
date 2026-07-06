using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.AppUpdates;

public sealed class JsonApplicationUpdateStateStore : IApplicationUpdateStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly ApplicationUpdateStateStoreOptions _options;

    public JsonApplicationUpdateStateStore(ApplicationUpdateStateStoreOptions options)
    {
        _options = options;
    }

    public async Task<ApplicationUpdateState> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return new ApplicationUpdateState(_options.CurrentVersion);
        }

        await using var stream = File.OpenRead(_options.FilePath);
        var state = await JsonSerializer.DeserializeAsync<ApplicationUpdateState>(stream, JsonOptions, cancellationToken);

        return state ?? new ApplicationUpdateState(_options.CurrentVersion);
    }

    public async Task SaveAsync(ApplicationUpdateState state, CancellationToken cancellationToken)
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
}
