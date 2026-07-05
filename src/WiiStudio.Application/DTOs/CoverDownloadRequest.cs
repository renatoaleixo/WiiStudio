namespace WiiStudio.Application.DTOs;

public sealed record CoverDownloadRequest(
    IReadOnlyList<string> GameIds,
    bool ForceRefresh = false,
    IReadOnlyList<CoverAssetKind>? AssetKinds = null);
