namespace WiiStudio.Application.DTOs;

public sealed record DiagnosticsCheckResult(
    string Name,
    DiagnosticsStatus Status,
    string Message,
    string? Version = null,
    string Category = "General",
    DiagnosticsSeverity Severity = DiagnosticsSeverity.Information);
