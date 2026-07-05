using System.IO;
using WiiStudio.Application.Abstractions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Infrastructure.Services;

public sealed class UsbDriveService : IUsbDriveService
{
    public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var drives = DriveInfo.GetDrives()
            .Where(drive => drive.IsReady && drive.DriveType is DriveType.Removable or DriveType.Fixed)
            .Select(MapDrive)
            .ToArray();

        return Task.FromResult<IReadOnlyList<UsbDrive>>(drives);
    }

    private static UsbDrive MapDrive(DriveInfo drive)
    {
        return new UsbDrive(
            string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel,
            drive.RootDirectory.FullName,
            MapFileSystem(drive.DriveFormat),
            drive.TotalSize,
            drive.AvailableFreeSpace);
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
}
