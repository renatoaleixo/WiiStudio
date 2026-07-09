namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WiimmsToolOptions
{
    public string? WitExecutablePath { get; init; }

    public string? WwtExecutablePath { get; init; }

    public string? PathEnvironment { get; init; } = Environment.GetEnvironmentVariable("PATH");

    public string WiimmsDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "tools", "wiimms");

    public string ToolsDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "tools");

    public string ApplicationDirectory { get; init; } = AppContext.BaseDirectory;
}
