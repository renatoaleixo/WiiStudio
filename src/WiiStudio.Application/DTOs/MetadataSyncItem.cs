namespace WiiStudio.Application.DTOs;

public sealed record MetadataSyncItem(
    string GameId,
    MetadataSyncStatus Status,
    MetadataSyncSource Source,
    GameMetadataDto? Metadata,
    string Message);
