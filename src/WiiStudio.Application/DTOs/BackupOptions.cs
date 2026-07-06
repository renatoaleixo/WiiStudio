namespace WiiStudio.Application.DTOs;

public sealed record BackupOptions(
    bool IncludeSettings = true,
    bool IncludeProfiles = true,
    bool IncludeFavorites = true,
    bool IncludeMetadata = true,
    bool IncludeNotifications = true,
    bool IncludeTasks = true,
    bool IncludeUpdates = true,
    bool IncludeCache = false,
    bool IncludeLogs = false);
