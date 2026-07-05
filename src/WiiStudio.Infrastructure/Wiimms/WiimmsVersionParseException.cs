namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WiimmsVersionParseException : Exception
{
    public WiimmsVersionParseException(string standardOutput)
        : base("Could not parse Wiimms version from command output.")
    {
        StandardOutput = standardOutput;
    }

    public string StandardOutput { get; }
}
