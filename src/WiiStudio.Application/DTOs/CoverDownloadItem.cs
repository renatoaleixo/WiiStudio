namespace WiiStudio.Application.DTOs;

public sealed record CoverDownloadItem(
    string GameId,
    CoverAssetKind Kind,
    CoverAssetSource Source,
    string? LocalPath,
    string Message);
