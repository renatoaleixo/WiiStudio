namespace WiiStudio.Application.DTOs;

public enum LocalCompatibilityStatus
{
    NotTested,
    WorksWell,
    WorksWithIssues,
    DoesNotWork,
    Unknown
}

public sealed record EmulatorCompatibilityDto(
    string GameId,
    string EmulatorId,
    LocalCompatibilityStatus Status,
    string? Notes,
    string? PreferredProfileId,
    DateTimeOffset UpdatedAt);

public sealed record SaveLocationResult(
    bool Found,
    string GameId,
    string EmulatorId,
    string? SavePath,
    string Message);

public sealed record SaveBackupRequest(
    string GameId,
    string EmulatorId,
    string DestinationFile,
    bool UserConfirmed);

public sealed record SaveBackupResult(
    bool Success,
    string? BackupFile,
    long SizeInBytes,
    DateTimeOffset CreatedAt,
    string Message);

public sealed record SaveBackupEntry(
    Guid Id,
    string GameId,
    string EmulatorId,
    string SourcePath,
    string BackupPath,
    long SizeInBytes,
    bool Success,
    string? Message,
    DateTimeOffset CreatedAt);
