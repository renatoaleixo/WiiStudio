namespace WiiStudio.Application.DTOs;

public sealed record PrivilegedProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    TimeSpan Duration,
    PrivilegedAccessStatus AccessStatus)
{
    public bool IsSuccess => ExitCode == 0;
}
