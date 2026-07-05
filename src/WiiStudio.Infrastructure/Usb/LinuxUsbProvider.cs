using System.IO;
using System.Security.Cryptography;
using System.Text;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Infrastructure.Usb;

public sealed class LinuxUsbProvider : IUsbProvider
{
    public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var drives = DriveInfo.GetDrives()
            .Where(IsCandidateDrive)
            .Select(MapDrive)
            .ToArray();

        return Task.FromResult<IReadOnlyList<UsbDrive>>(drives);
    }

    private static bool IsCandidateDrive(DriveInfo drive)
    {
        if (!drive.IsReady || drive.DriveType is not (DriveType.Removable or DriveType.Fixed))
        {
            return false;
        }

        var fileSystem = MapFileSystem(drive.DriveFormat);
        return fileSystem == UsbDriveFileSystem.Wbfs || HasWbfsDirectory(drive);
    }

    private static UsbDrive MapDrive(DriveInfo drive)
    {
        var mountPoint = drive.RootDirectory.FullName;
        var label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel;
        var freeSpace = drive.AvailableFreeSpace;
        var usedSpace = drive.TotalSize - freeSpace;

        return new UsbDrive(
            CreateStableId(mountPoint),
            mountPoint,
            mountPoint,
            label,
            MapFileSystem(drive.DriveFormat),
            drive.TotalSize,
            usedSpace,
            freeSpace,
            isMounted: true,
            isWritable: true);
    }

    private static bool HasWbfsDirectory(DriveInfo drive)
    {
        return Directory.Exists(Path.Combine(drive.RootDirectory.FullName, "wbfs"));
    }

    private static UsbDriveFileSystem MapFileSystem(string driveFormat)
    {
        return driveFormat.ToUpperInvariant() switch
        {
            "WBFS" => UsbDriveFileSystem.Wbfs,
            "FAT32" => UsbDriveFileSystem.Fat32,
            "NTFS" => UsbDriveFileSystem.Ntfs,
            "EXFAT" => UsbDriveFileSystem.ExFat,
            _ => UsbDriveFileSystem.Unknown
        };
    }

    private static Guid CreateStableId(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim().ToUpperInvariant()));
        return new Guid(hash[..16]);
    }
}
