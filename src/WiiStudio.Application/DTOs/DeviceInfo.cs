using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record DeviceInfo(
    string DeviceId,
    string Name,
    string DevicePath,
    string MountPoint,
    string Manufacturer,
    string Model,
    string? Serial,
    string Interface,
    UsbDriveFileSystem FileSystem,
    long Capacity,
    long UsedSpace,
    long FreeSpace,
    DeviceStatus Status,
    bool IsWiiCompatible,
    DateTimeOffset RefreshedAt);
