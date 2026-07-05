namespace WiiStudio.Infrastructure.Processes;

public sealed class ProcessResult
{
    public ProcessResult(int exitCode, string standardOutput, string standardError, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(standardOutput);
        ArgumentNullException.ThrowIfNull(standardError);

        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
        Duration = duration;
    }

    public int ExitCode { get; }

    public string StandardOutput { get; }

    public string StandardError { get; }

    public TimeSpan Duration { get; }

    public bool IsSuccess => ExitCode == 0;
}
