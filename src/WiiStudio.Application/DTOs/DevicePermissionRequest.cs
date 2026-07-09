namespace WiiStudio.Application.DTOs;

public sealed record DevicePermissionRequest(
    string ToolId,
    string ExecutablePath,
    string? DevicePath,
    PrivilegedDeviceOperation Operation);
