using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Usb;

public sealed class LinuxUsbDeviceProvider : IPlatformUsbDeviceProvider
{
    private readonly LinuxUsbProvider _linuxUsbProvider;
    private readonly IDevicePermissionService _devicePermissionService;
    private readonly IWiimmsToolLocator _toolLocator;

    public LinuxUsbDeviceProvider(
        LinuxUsbProvider linuxUsbProvider,
        IDevicePermissionService devicePermissionService,
        IWiimmsToolLocator toolLocator)
    {
        _linuxUsbProvider = linuxUsbProvider;
        _devicePermissionService = devicePermissionService;
        _toolLocator = toolLocator;
    }

    public OSPlatformKind Platform => OSPlatformKind.Linux;

    public async Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var drives = await _linuxUsbProvider.DetectAsync(cancellationToken);
        var wwtPath = await TryFindWwtAsync(cancellationToken);
        var devices = new List<UsbDeviceInfo>();

        foreach (var drive in drives)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var permission = wwtPath is null
                ? null
                : await _devicePermissionService.CheckAsync(
                    new DevicePermissionRequest("wwt", wwtPath, drive.DevicePath, PrivilegedDeviceOperation.ListWbfsGames),
                    cancellationToken);
            devices.Add(MapDrive(drive, permission, isUsb: IsLikelyUsb(drive), isRemovable: true));
        }

        return devices;
    }

    private static UsbDeviceInfo MapDrive(
        UsbDrive drive,
        DevicePermissionStatus? permission,
        bool isUsb,
        bool isRemovable)
    {
        var fileSystem = drive.FileSystem.ToString();
        var partition = new UsbPartitionInfo(
            drive.Id.ToString(),
            drive.DevicePath,
            drive.MountPoint,
            fileSystem,
            drive.Capacity,
            drive.FreeSpace,
            drive.FileSystem == UsbDriveFileSystem.Wbfs,
            drive.IsWritable);

        return new UsbDeviceInfo(
            drive.Id.ToString(),
            drive.Label,
            drive.DevicePath,
            drive.MountPoint,
            isUsb,
            isRemovable,
            drive.Capacity,
            [partition],
            permission);
    }

    private async Task<string?> TryFindWwtAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _toolLocator.FindWwtAsync(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsLikelyUsb(UsbDrive drive)
    {
        return drive.DevicePath.StartsWith("/dev/sd", StringComparison.OrdinalIgnoreCase)
            || drive.DevicePath.StartsWith("/media/", StringComparison.OrdinalIgnoreCase)
            || drive.DevicePath.StartsWith("/run/media/", StringComparison.OrdinalIgnoreCase);
    }
}
