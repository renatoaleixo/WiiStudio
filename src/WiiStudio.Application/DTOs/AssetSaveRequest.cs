namespace WiiStudio.Application.DTOs;

public sealed record AssetSaveRequest(
    AssetKey Key,
    Stream Content);
