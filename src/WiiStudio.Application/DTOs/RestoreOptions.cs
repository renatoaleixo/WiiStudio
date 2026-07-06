namespace WiiStudio.Application.DTOs;

public sealed record RestoreOptions(
    bool RestoreSettings = true,
    bool RestoreProfiles = true,
    bool RestoreFavorites = true,
    bool RestoreMetadata = true,
    bool RestoreNotifications = true,
    bool RestoreTasks = true,
    bool RestoreUpdates = true,
    bool RestoreCache = false,
    bool RestoreLogs = false);
