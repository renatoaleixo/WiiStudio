namespace WiiStudio.Application.DTOs;

public sealed record LogCleanupOptions(
    DateTimeOffset? OlderThan = null,
    LogLevel? MinimumLevelToKeep = null);
