using System.IO;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Privilege;

namespace WiiStudio.Infrastructure.Usb;

public sealed class WindowsUsbDeviceProvider : IPlatformUsbDeviceProvider
{
    private readonly IWindowsElevationDetector _elevationDetector;

    public WindowsUsbDeviceProvider(IWindowsElevationDetector elevationDetector)
    {
        _elevationDetector = elevationDetector;
    }

    public OSPlatformKind Platform => OSPlatformKind.Windows;

    public Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var permission = new DevicePermissionStatus(
            OSPlatformKind.Windows,
            _elevationDetector.IsAdministrator() ? PrivilegedAccessState.Authorized : PrivilegedAccessState.AuthorizationRequired,
            CanRetry: true,
            RequiresUserAction: !_elevationDetector.IsAdministrator(),
            _elevationDetector.IsAdministrator() ? "Sessão administrativa disponível." : "Execute como administrador para acesso raw a discos físicos.",
            _elevationDetector.IsAdministrator() ? "Nenhuma ação necessária." : "Reabra o WiiStudio como Administrador.",
            "Windows DriveInfo provider.");

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
        "FAT32" => UsbDriveFileSystem.Fat32.ToString(),
        "NTFS" => UsbDriveFileSystem.Ntfs.ToString(),
        "EXFAT" => UsbDriveFileSystem.ExFat.ToString(),
        "WBFS" => UsbDriveFileSystem.Wbfs.ToString(),
        _ => UsbDriveFileSystem.Unknown.ToString()
    };
}
