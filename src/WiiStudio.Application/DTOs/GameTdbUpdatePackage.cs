namespace WiiStudio.Application.DTOs;

public sealed record GameTdbUpdatePackage(
    GameTdbUpdateInfo Info,
    IReadOnlyList<GameMetadataDto> Metadata,
    string? Checksum = null);
