namespace WiiStudio.Application.DTOs;

public sealed record CoverAssetBulkActionRequest(
    IReadOnlyList<string> GameIds,
    IReadOnlyList<CoverAssetKind> AssetKinds,
    string? Language,
    bool ForceRefresh,
    bool ClearBeforeDownload,
    CoverAssetBulkActionMode Mode);
