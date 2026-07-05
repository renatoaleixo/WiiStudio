using WiiStudio.Domain.Enums;

namespace WiiStudio.Domain.Entities;

public sealed class UsbDrive
{
    public UsbDrive(string name, string mountPath, UsbDriveFileSystem fileSystem, long totalSizeInBytes, long availableSizeInBytes)
        : this(Guid.NewGuid(), mountPath, mountPath, name, fileSystem, totalSizeInBytes, totalSizeInBytes - availableSizeInBytes, availableSizeInBytes, true, true)
    {
    }

    public UsbDrive(
        Guid id,
        string devicePath,
        string mountPoint,
        string label,
        UsbDriveFileSystem fileSystem,
        long capacity,
        long usedSpace,
        long freeSpace,
        bool isMounted,
        bool isWritable)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("USB drive id cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(devicePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(mountPoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(usedSpace);
        ArgumentOutOfRangeException.ThrowIfNegative(freeSpace);

        if (usedSpace + freeSpace > capacity)
        {
            throw new ArgumentException("Used space and free space cannot exceed capacity.", nameof(usedSpace));
        }

        Id = id;
        DevicePath = devicePath.Trim();
        MountPoint = mountPoint.Trim();
        Label = label.Trim();
        FileSystem = fileSystem;
        Capacity = capacity;
        UsedSpace = usedSpace;
        FreeSpace = freeSpace;
        IsMounted = isMounted;
        IsWritable = isWritable;
    }

    public Guid Id { get; }

    public string DevicePath { get; }

    public string MountPoint { get; }

    public string Label { get; }

    public string Name => Label;

    public string MountPath => MountPoint;

    public UsbDriveFileSystem FileSystem { get; }

    public long Capacity { get; }

    public long UsedSpace { get; }

    public long FreeSpace { get; }

    public long TotalSizeInBytes => Capacity;

    public long AvailableSizeInBytes => FreeSpace;

    public bool IsMounted { get; }

    public bool IsWritable { get; }

    public bool IsWbfs => FileSystem == UsbDriveFileSystem.Wbfs;
}
