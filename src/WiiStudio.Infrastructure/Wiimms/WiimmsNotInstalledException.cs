namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WiimmsNotInstalledException : Exception
{
    public WiimmsNotInstalledException(string toolName)
        : base($"Wiimms executable '{toolName}' was not found.")
    {
        ToolName = toolName;
    }

    public string ToolName { get; }
}
