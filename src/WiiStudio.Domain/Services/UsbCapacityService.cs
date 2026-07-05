using WiiStudio.Domain.Entities;

namespace WiiStudio.Domain.Services;

public sealed class UsbCapacityService
{
    public bool HasEnoughSpace(UsbDrive drive, long requiredBytes)
    {
        ArgumentNullException.ThrowIfNull(drive);
        ArgumentOutOfRangeException.ThrowIfNegative(requiredBytes);

        return drive.FreeSpace >= requiredBytes;
    }
}
