namespace WiiStudio.Application.DTOs;

public sealed record GameTdbUpdateState(
    string? InstalledVersion = null,
    string? PreviousVersion = null,
    DateTimeOffset? LastCheckedAt = null,
    DateTimeOffset? LastUpdatedAt = null,
    string? LastMirror = null);
