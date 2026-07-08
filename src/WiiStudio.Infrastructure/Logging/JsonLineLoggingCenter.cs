using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Security;

namespace WiiStudio.Infrastructure.Logging;

public sealed class JsonLineLoggingCenter : ILoggingCenter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly LoggingCenterOptions _options;
    private readonly ILogRedactionService _redactionService;

    public JsonLineLoggingCenter(LoggingCenterOptions options, ILogRedactionService? redactionService = null)
    {
        _options = options;
        _redactionService = redactionService ?? new LogRedactionService();
    }

    public async Task RecordAsync(LogEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Validate(entry);
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(_options.FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        await JsonSerializer.SerializeAsync(stream, Mask(entry), JsonOptions, cancellationToken);
        await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
    }

    public async Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var entries = await LoadEntriesAsync(cancellationToken);
        var filtered = entries.AsEnumerable();

        if (query.MinimumLevel is not null)
        {
            filtered = filtered.Where(entry => entry.Level >= query.MinimumLevel);
        }

        if (!string.IsNullOrWhiteSpace(query.Source))
        {
            filtered = filtered.Where(entry => string.Equals(entry.Source, query.Source, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            filtered = filtered.Where(entry => string.Equals(entry.Category, query.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.OperationId))
        {
            filtered = filtered.Where(entry => string.Equals(entry.OperationId, query.OperationId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            filtered = filtered.Where(entry =>
                entry.Message.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase) ||
                (entry.Exception?.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (query.From is not null)
        {
            filtered = filtered.Where(entry => entry.Timestamp >= query.From);
        }

        if (query.To is not null)
        {
            filtered = filtered.Where(entry => entry.Timestamp <= query.To);
        }

        var limit = Math.Max(1, query.Limit ?? _options.DefaultQueryLimit);

        return filtered
            .OrderByDescending(entry => entry.Timestamp)
            .Take(limit)
            .ToArray();
    }

    public async Task ExportAsync(LogExportOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidatePath(options.OutputFile, nameof(options.OutputFile));
        cancellationToken.ThrowIfCancellationRequested();

        var entries = await QueryAsync(options.Query ?? new LogQuery(), cancellationToken);
        var directory = Path.GetDirectoryName(options.OutputFile);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(options.OutputFile);
        await JsonSerializer.SerializeAsync(stream, entries, new JsonSerializerOptions { WriteIndented = true }, cancellationToken);
    }

    public async Task ClearAsync(LogCleanupOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return;
        }

        if (options.OlderThan is null && options.MinimumLevelToKeep is null)
        {
            File.Delete(_options.FilePath);
            return;
        }

        var entries = await LoadEntriesAsync(cancellationToken);
        var keptEntries = entries.Where(entry => ShouldKeep(entry, options)).ToArray();
        await RewriteEntriesAsync(keptEntries, cancellationToken);
    }

    private async Task<IReadOnlyList<LogEntry>> LoadEntriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return [];
        }

        var entries = new List<LogEntry>();
        foreach (var line in await File.ReadAllLinesAsync(_options.FilePath, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var entry = JsonSerializer.Deserialize<LogEntry>(line, JsonOptions);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private async Task RewriteEntriesAsync(IReadOnlyList<LogEntry> entries, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_options.FilePath);
        foreach (var entry in entries.OrderBy(entry => entry.Timestamp))
        {
            await JsonSerializer.SerializeAsync(stream, entry, JsonOptions, cancellationToken);
            await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        }
    }

    private static bool ShouldKeep(LogEntry entry, LogCleanupOptions options)
    {
        if (options.OlderThan is not null && entry.Timestamp < options.OlderThan)
        {
            return false;
        }

        if (options.MinimumLevelToKeep is not null && entry.Level < options.MinimumLevelToKeep)
        {
            return false;
        }

        return true;
    }

    private LogEntry Mask(LogEntry entry)
    {
        return entry with
        {
            Message = _redactionService.Redact(entry.Message),
            Exception = entry.Exception is null ? null : _redactionService.Redact(entry.Exception),
            Properties = entry.Properties is null ? null : _redactionService.Redact(entry.Properties)
        };
    }

    private static void Validate(LogEntry entry)
    {
        if (entry.Id == Guid.Empty)
        {
            throw new ArgumentException("Log id must be provided.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.Source))
        {
            throw new ArgumentException("Log source must be provided.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.Category))
        {
            throw new ArgumentException("Log category must be provided.", nameof(entry));
        }

        if (string.IsNullOrWhiteSpace(entry.Message))
        {
            throw new ArgumentException("Log message must be provided.", nameof(entry));
        }
    }

    private static void ValidatePath(string filePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Path must be provided.", parameterName);
        }

        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException("Path contains invalid characters.", parameterName);
        }
    }
}
