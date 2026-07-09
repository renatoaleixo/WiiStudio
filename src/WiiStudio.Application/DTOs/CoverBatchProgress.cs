namespace WiiStudio.Application.DTOs;

public sealed record CoverBatchProgress(
    int TotalGames,
    int ProcessedGames,
    string? CurrentGameId,
    string? CurrentTitle,
    CoverAssetKind? CurrentAssetKind,
    CoverAssetViewState CurrentState);
