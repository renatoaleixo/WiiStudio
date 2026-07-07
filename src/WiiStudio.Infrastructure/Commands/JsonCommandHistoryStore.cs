using System.Text.Json;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Commands;

public sealed class JsonCommandHistoryStore : ICommandHistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly CommandHistoryStoreOptions _options;

    public JsonCommandHistoryStore(CommandHistoryStoreOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<string>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_options.FilePath);
        return await JsonSerializer.DeserializeAsync<string[]>(stream, JsonOptions, cancellationToken) ?? [];
    }

    public async Task SaveAsync(IReadOnlyList<string> commandIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(commandIds);
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_options.FilePath);
        await JsonSerializer.SerializeAsync(stream, commandIds, JsonOptions, cancellationToken);
    }
}
