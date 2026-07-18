namespace WiiStudio.Application.DTOs;

public sealed record ImportHistoryEntry(
    Guid Id,
    string? GameId,
    string? SourcePath,
    string? TargetDeviceId,
    string Operation,
    bool Success,
    string? Message,
    DateTimeOffset CreatedAt);
