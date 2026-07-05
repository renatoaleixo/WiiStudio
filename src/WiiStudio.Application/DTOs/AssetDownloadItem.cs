namespace WiiStudio.Application.DTOs;

public sealed record AssetDownloadItem(
    string GameId,
    CoverAssetKind Kind,
    AssetDownloadStatus Status,
    CoverAssetSource Source,
    AssetInfo? Asset,
    string Message);
