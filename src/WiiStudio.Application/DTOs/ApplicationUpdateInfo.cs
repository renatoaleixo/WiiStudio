namespace WiiStudio.Application.DTOs;

public sealed record ApplicationUpdateInfo(
    string Version,
    string CurrentVersion,
    bool IsAvailable,
    DateTimeOffset CheckedAt,
    string? DownloadUrl = null,
    string? Changelog = null,
    string? Signature = null,
    string? Checksum = null);
