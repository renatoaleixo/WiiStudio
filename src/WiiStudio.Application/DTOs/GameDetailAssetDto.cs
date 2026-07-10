namespace WiiStudio.Application.DTOs;

public sealed record GameDetailAssetDto(
    CoverAssetKind Kind,
    string DisplayName,
    string? LocalPath,
    bool Exists,
    long SizeInBytes = 0);
