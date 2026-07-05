using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Mappers;

public static class UsbDriveMapper
{
    public static UsbDriveDto ToDto(UsbDrive drive)
    {
        ArgumentNullException.ThrowIfNull(drive);

        return new UsbDriveDto(
            drive.Id,
            drive.DevicePath,
            drive.MountPoint,
            drive.Label,
            drive.FileSystem,
            drive.Capacity,
            drive.UsedSpace,
            drive.FreeSpace,
            drive.IsMounted,
            drive.IsWritable);
    }
}
