namespace WiiStudio.Infrastructure.Processes;

public sealed class ProcessRequest
{
    public ProcessRequest(
        string executablePath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        Action<string>? onStandardOutputLine = null,
        Action<string>? onStandardErrorLine = null)
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
        OnStandardOutputLine = onStandardOutputLine;
        OnStandardErrorLine = onStandardErrorLine;
    }

    public string ExecutablePath { get; }

    public IReadOnlyList<string> Arguments { get; }

    public TimeSpan Timeout { get; }

    public Action<string>? OnStandardOutputLine { get; }

    public Action<string>? OnStandardErrorLine { get; }
}
