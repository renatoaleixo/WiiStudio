namespace WiiStudio.Application.DTOs;

public sealed record UpdateResult(
    GameTdbUpdateStatus Status,
    string? InstalledVersion,
    string? PreviousVersion,
    int MetadataUpdated,
    DateTimeOffset CompletedAt,
    string Message);
