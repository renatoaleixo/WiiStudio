namespace WiiStudio.Application.DTOs;

public sealed record MetadataCacheStatistics(
    int TotalGames,
    long SizeInBytes,
    DateTimeOffset? LastUpdatedAt);
