namespace WiiStudio.Application.DTOs;

public sealed record AssetStatistics(
    int TotalAssets,
    long SizeInBytes,
    IReadOnlyDictionary<CoverAssetKind, int> CountByKind,
    DateTimeOffset? LastModifiedAt);
