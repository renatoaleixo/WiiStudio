namespace WiiStudio.Application.DTOs;

public sealed record AssetRequest(
    AssetKey Key,
    bool AllowProviderFallback = true,
    bool ForceRefresh = false);
