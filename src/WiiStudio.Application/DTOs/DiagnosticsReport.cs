namespace WiiStudio.Application.DTOs;

public sealed record DiagnosticsReport(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<DiagnosticsCheckResult> Checks)
{
    public bool IsHealthy => Checks.All(check => check.Status is not DiagnosticsStatus.Unhealthy);
}
