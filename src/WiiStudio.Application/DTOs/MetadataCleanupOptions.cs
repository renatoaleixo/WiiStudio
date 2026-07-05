namespace WiiStudio.Application.DTOs;

public sealed record MetadataCleanupOptions(
    bool RemoveExpiredOnly = true,
    TimeSpan? Expiration = null);
