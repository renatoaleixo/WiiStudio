namespace WiiStudio.Application.DTOs;

public sealed record AssetInfo(
    AssetKey Key,
    string LocalPath,
    long SizeInBytes,
    DateTimeOffset? LastModifiedAt,
    CoverAssetSource Source);
