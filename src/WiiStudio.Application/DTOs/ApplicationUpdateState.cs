namespace WiiStudio.Application.DTOs;

public sealed record ApplicationUpdateState(
    string CurrentVersion,
    string? PendingVersion = null,
    string? PreviousVersion = null,
    string? PackagePath = null,
    bool IsDownloaded = false,
    bool IsValidated = false,
    bool RestartPending = false,
    DateTimeOffset? LastCheckedAt = null,
    DateTimeOffset? LastInstalledAt = null);
