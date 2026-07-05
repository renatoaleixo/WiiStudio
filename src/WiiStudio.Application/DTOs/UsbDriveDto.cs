using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record UsbDriveDto(
    Guid Id,
    string DevicePath,
    string MountPoint,
    string Label,
    UsbDriveFileSystem FileSystem,
    long Capacity,
    long UsedSpace,
    long FreeSpace,
    bool IsMounted,
    bool IsWritable);
