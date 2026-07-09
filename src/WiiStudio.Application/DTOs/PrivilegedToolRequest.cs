namespace WiiStudio.Application.DTOs;

public sealed record PrivilegedToolRequest(
    string ToolId,
    string ExecutablePath,
    IReadOnlyList<string> Arguments,
    TimeSpan Timeout,
    PrivilegedDeviceOperation Operation,
    string? WorkingDirectory = null,
    string? DevicePath = null,
    string? CorrelationId = null);
