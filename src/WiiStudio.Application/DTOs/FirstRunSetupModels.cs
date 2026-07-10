namespace WiiStudio.Application.DTOs;

public enum FirstRunSetupStep
{
    Welcome = 1,
    Language = 2,
    Theme = 3,
    Folders = 4,
    ExternalTools = 5,
    Permissions = 6,
    GameTdb = 7,
    Summary = 8,
    Completed = 9,
    Skipped = 10
}

public sealed record FirstRunSetupStatus(
    bool IsCompleted,
    DateTimeOffset? CompletedAt,
    FirstRunSetupStep CurrentStep,
    IReadOnlyList<string> Summary);

public sealed record FirstRunSetupRequest(
    string Language,
    string Theme,
    string? LibraryPath,
    string? ExportPath,
    bool UserConfirmedDownloads,
    bool Skip = false);

public sealed record FirstRunSetupResult(
    bool Completed,
    bool Skipped,
    DateTimeOffset CompletedAt,
    IReadOnlyList<string> Summary);
