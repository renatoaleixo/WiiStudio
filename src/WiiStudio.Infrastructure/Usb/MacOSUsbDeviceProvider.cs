using System.IO;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Infrastructure.Usb;

public sealed class MacOSUsbDeviceProvider : IPlatformUsbDeviceProvider
{
    public OSPlatformKind Platform => OSPlatformKind.MacOS;

    public Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var permission = new DevicePermissionStatus(
            OSPlatformKind.MacOS,
            PrivilegedAccessState.AuthorizationRequired,
            CanRetry: true,
            RequiresUserAction: true,
            "macOS pode exigir autorização para acesso raw a discos físicos.",
            "Conceda permissões ao aplicativo quando solicitado.",
            "macOS mounted volume provider.");
        var devices = DriveInfo.GetDrives()
            .Where(drive => drive.IsReady && drive.DriveType is DriveType.Removable or DriveType.Fixed)
            .Select(drive => MapDrive(drive, permission))
            .ToArray();

        return Task.FromResult<IReadOnlyList<UsbDeviceInfo>>(devices);
    }

    private static UsbDeviceInfo MapDrive(DriveInfo drive, DevicePermissionStatus permission)
    {
        var fileSystem = NormalizeFileSystem(drive.DriveFormat);
        var partition = new UsbPartitionInfo(
            drive.Name,
            drive.Name,
            drive.RootDirectory.FullName,
            fileSystem,
            drive.TotalSize,
            drive.AvailableFreeSpace,
            string.Equals(fileSystem, UsbDriveFileSystem.Wbfs.ToString(), StringComparison.OrdinalIgnoreCase),
            true);

        return new UsbDeviceInfo(
            drive.Name,
            string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel,
            drive.Name,
            drive.RootDirectory.FullName,
            IsUsb: drive.DriveType == DriveType.Removable,
            IsRemovable: drive.DriveType == DriveType.Removable,
            drive.TotalSize,
            [partition],
            permission);
    }

    private static string NormalizeFileSystem(string value) => value.ToUpperInvariant() switch
    {
        "MS-DOS FAT32" or "FAT32" => UsbDriveFileSystem.Fat32.ToString(),
        "EXFAT" => UsbDriveFileSystem.ExFat.ToString(),
        "NTFS" => UsbDriveFileSystem.Ntfs.ToString(),
        "WBFS" => UsbDriveFileSystem.Wbfs.ToString(),
        _ => UsbDriveFileSystem.Unknown.ToString()
    };
}
