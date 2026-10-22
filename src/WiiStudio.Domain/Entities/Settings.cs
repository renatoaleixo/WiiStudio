namespace WiiStudio.Domain.Entities;

public sealed class Settings
{
    public string Language { get; init; } = "pt-BR";

    public string Theme { get; init; } = "System";

    public bool AutoScanLibrary { get; init; } = true;

    public string? LibraryPath { get; init; }

    public string? ExportPath { get; init; }

    public int MaxParallelTasks { get; init; } = 2;

    public bool EnableDiagnostics { get; init; } = true;

    public bool CheckForUpdatesOnStartup { get; init; } = true;

    public string? WitExecutablePath { get; init; }

    public string? WwtExecutablePath { get; init; }

    public string? NKitExecutablePath { get; init; }

    public string? DolphinExecutablePath { get; init; }

    public string? WitVersion { get; init; }

    public string? WwtVersion { get; init; }

    public string? NKitVersion { get; init; }

    public string? DolphinVersion { get; init; }

    public bool EmulatorShowPreviewBeforeLaunch { get; init; } = true;

    public bool EmulatorSaveLaunchHistory { get; init; } = true;

    public bool EmulatorCloseAfterLaunch { get; init; }

    public string? EmulatorLastId { get; init; }

    public string? EmulatorLastGameId { get; init; }

    public DateTimeOffset? ExternalToolsLastCheckAt { get; init; }

    public string? CoverCachePath { get; init; }

    public string? MetadataCachePath { get; init; }

    public bool FirstRunCompleted { get; init; }

    public DateTimeOffset? FirstRunCompletedAt { get; init; }
}
