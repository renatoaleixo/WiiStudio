namespace WiiStudio.Application.DTOs;

public sealed record CoverAssetBulkActionProgress(
    int TotalGames,
    int ProcessedGames,
    string? CurrentGameId,
    CoverAssetKind? CurrentAssetKind,
    string State);
