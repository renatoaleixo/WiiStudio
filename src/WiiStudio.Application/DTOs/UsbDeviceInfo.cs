namespace WiiStudio.Application.DTOs;

public sealed record UsbDeviceInfo(
    string DeviceId,
    string DisplayName,
    string? PhysicalPath,
    string? MountPath,
    bool IsUsb,
    bool IsRemovable,
    long? SizeBytes,
    IReadOnlyList<UsbPartitionInfo> Partitions,
    DevicePermissionStatus? PermissionStatus = null);
