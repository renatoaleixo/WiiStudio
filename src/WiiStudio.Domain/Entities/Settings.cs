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

    public string? CoverCachePath { get; init; }

    public string? MetadataCachePath { get; init; }
}
