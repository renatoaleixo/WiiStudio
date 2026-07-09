namespace WiiStudio.Application.DTOs;

public sealed record CoverBatchDownloadSummary(
    int Requested,
    int Downloaded,
    int FromCache,
    int Missing,
    int Failed,
    int ProcessedGames,
    bool Cancelled = false);
