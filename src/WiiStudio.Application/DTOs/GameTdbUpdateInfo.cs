namespace WiiStudio.Application.DTOs;

public sealed record GameTdbUpdateInfo(
    string Version,
    DateTimeOffset PublishedAt,
    string Mirror,
    string? Checksum = null,
    bool SupportsIncrementalUpdate = false);
