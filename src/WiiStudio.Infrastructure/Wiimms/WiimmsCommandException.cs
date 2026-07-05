namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WiimmsCommandException : Exception
{
    public WiimmsCommandException(string message, int exitCode, string standardError)
        : base(message)
    {
        ExitCode = exitCode;
        StandardError = standardError;
    }

    public int ExitCode { get; }

    public string StandardError { get; }
}
