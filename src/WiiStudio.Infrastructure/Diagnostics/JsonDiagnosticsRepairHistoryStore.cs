using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Diagnostics;

public sealed class JsonDiagnosticsRepairHistoryStore : IDiagnosticsRepairHistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly DiagnosticsRepairHistoryStoreOptions _options;

    public JsonDiagnosticsRepairHistoryStore(DiagnosticsRepairHistoryStoreOptions options)
    {
        _options = options;
    }

    public async Task AppendAsync(DiagnosticRepairHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();

        var history = await LoadAsync(cancellationToken);
        history.Add(entry);
        await SaveAsync(ApplyHistoryLimit(history), cancellationToken);
    }

    public async Task<IReadOnlyList<DiagnosticRepairHistoryEntry>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var history = await LoadAsync(cancellationToken);

        return history
            .OrderByDescending(entry => entry.FinishedAt)
            .Take(Math.Max(1, limit))
            .ToArray();
    }

    private async Task<List<DiagnosticRepairHistoryEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_options.FilePath);
        var entries = await JsonSerializer.DeserializeAsync<DiagnosticRepairHistoryEntry[]>(stream, JsonOptions, cancellationToken);

        return (entries ?? [])
            .Where(IsValid)
            .OrderBy(entry => entry.FinishedAt)
            .ToList();
    }

    private async Task SaveAsync(IReadOnlyList<DiagnosticRepairHistoryEntry> history, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_options.FilePath);
        await JsonSerializer.SerializeAsync(stream, history, JsonOptions, cancellationToken);
    }

    private IReadOnlyList<DiagnosticRepairHistoryEntry> ApplyHistoryLimit(List<DiagnosticRepairHistoryEntry> history)
    {
        var limit = Math.Max(1, _options.HistoryLimit);

        return history
            .OrderByDescending(entry => entry.FinishedAt)
            .Take(limit)
            .OrderBy(entry => entry.FinishedAt)
            .ToArray();
    }

    private static bool IsValid(DiagnosticRepairHistoryEntry entry)
    {
        return entry.Id != Guid.Empty
            && !string.IsNullOrWhiteSpace(entry.ActionId)
            && !string.IsNullOrWhiteSpace(entry.Title);
    }
}
