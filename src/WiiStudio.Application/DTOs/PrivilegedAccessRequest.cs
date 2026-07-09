namespace WiiStudio.Application.DTOs;

public sealed record PrivilegedAccessRequest(
    string ToolId,
    string ExecutablePath,
    string? DevicePath,
    PrivilegedDeviceOperation Operation);
