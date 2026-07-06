namespace WiiStudio.Application.DTOs;

public sealed record LogEntry(
    Guid Id,
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Source,
    string Category,
    string Message,
    string? OperationId = null,
    string? Exception = null,
    IReadOnlyDictionary<string, string>? Properties = null);
