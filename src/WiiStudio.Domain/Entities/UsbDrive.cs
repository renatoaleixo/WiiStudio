using WiiStudio.Domain.Enums;

namespace WiiStudio.Domain.Entities;

public sealed class UsbDrive
{
    public UsbDrive(string name, string mountPath, UsbDriveFileSystem fileSystem, long totalSizeInBytes, long availableSizeInBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(mountPath);
        ArgumentOutOfRangeException.ThrowIfNegative(totalSizeInBytes);
        ArgumentOutOfRangeException.ThrowIfNegative(availableSizeInBytes);

        Name = name.Trim();
        MountPath = mountPath.Trim();
        FileSystem = fileSystem;
        TotalSizeInBytes = totalSizeInBytes;
        AvailableSizeInBytes = availableSizeInBytes;
    }

    public string Name { get; }

    public string MountPath { get; }

    public UsbDriveFileSystem FileSystem { get; }

    public long TotalSizeInBytes { get; }

    public long AvailableSizeInBytes { get; }

    public bool IsWbfs => FileSystem == UsbDriveFileSystem.Wbfs;
}
