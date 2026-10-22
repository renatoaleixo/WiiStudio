namespace WiiStudio.Application.DTOs;

public sealed record EmulatorLaunchHistoryEntry(
    Guid Id,
    string GameId,
    string EmulatorId,
    string? FilePath,
    DateTimeOffset StartedAt,
    int? ExitCode,
    string Result,
    string? ErrorMessage);
