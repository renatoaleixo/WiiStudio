namespace WiiStudio.Infrastructure.ExternalTools;

public sealed class ExternalToolOptions
{
    public string ToolsRootDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "tools");

    public string PackagesDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "tools", "packages");

    public string TemporaryDirectory { get; init; } = Path.Combine(Path.GetTempPath(), "WiiStudio", "external-tools");

    public long MaxDownloadBytes { get; init; } = 200L * 1024L * 1024L;

    public int DownloadTimeoutSeconds { get; init; } = 120;
}
