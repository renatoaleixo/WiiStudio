namespace WiiStudio.Application.DTOs;

public sealed record CoverAssetBulkActionResult(
    int RequestedGames,
    int RequestedAssets,
    int Downloaded,
    int FromCache,
    int Cleared,
    int Missing,
    int Failed,
    int Invalid,
    bool Cancelled);
