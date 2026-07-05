namespace WiiStudio.Infrastructure.Processes;

public sealed class ProcessRequest
{
    public ProcessRequest(string executablePath, IReadOnlyList<string> arguments, TimeSpan timeout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentNullException.ThrowIfNull(arguments);

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        ExecutablePath = executablePath;
        Arguments = arguments;
        Timeout = timeout;
    }

    public string ExecutablePath { get; }

    public IReadOnlyList<string> Arguments { get; }

    public TimeSpan Timeout { get; }
}
