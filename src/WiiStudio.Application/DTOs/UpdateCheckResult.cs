namespace WiiStudio.Application.DTOs;

public sealed record UpdateCheckResult(
    bool IsAvailable,
    string? InstalledVersion,
    GameTdbUpdateInfo? Latest,
    DateTimeOffset CheckedAt,
    string Message);
