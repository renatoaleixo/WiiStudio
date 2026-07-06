using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Services;

public sealed class DeviceManagerService : IDeviceManagerService
{
    private readonly IUsbDriveService _usbDriveService;
    private readonly Dictionary<string, DeviceInfo> _devices = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _ejectedDeviceIds = new(StringComparer.OrdinalIgnoreCase);

    public DeviceManagerService(IUsbDriveService usbDriveService)
    {
        _usbDriveService = usbDriveService;
    }

    public async Task<DeviceManagerSnapshot> RefreshAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var drives = await _usbDriveService.DetectAsync(cancellationToken);
        _devices.Clear();

        foreach (var drive in drives)
        {
            var device = CreateDeviceInfo(drive, _ejectedDeviceIds.Contains(drive.Id.ToString()));
            _devices[device.DeviceId] = device;
        }

        return new DeviceManagerSnapshot(_devices.Values.ToArray(), DeviceManagerState.Ready, DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_devices.Count == 0)
        {
            var snapshot = await RefreshAsync(cancellationToken);
            return snapshot.Devices;
        }

        return _devices.Values.ToArray();
    }

    public async Task<DeviceInfo> EjectAsync(string deviceId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        cancellationToken.ThrowIfCancellationRequested();

        var devices = await GetDevicesAsync(cancellationToken);
        var device = devices.FirstOrDefault(candidate => candidate.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Device {deviceId} was not found.");

        _ejectedDeviceIds.Add(device.DeviceId);
        var ejected = device with
        {
            Status = DeviceStatus.Ejected,
            RefreshedAt = DateTimeOffset.UtcNow
        };
        _devices[device.DeviceId] = ejected;

        return ejected;
    }

    private static DeviceInfo CreateDeviceInfo(UsbDrive drive, bool wasEjected)
    {
        var isCompatible = IsCompatibleFileSystem(drive.FileSystem);
        var status = wasEjected
            ? DeviceStatus.Ejected
            : GetStatus(drive, isCompatible);

        return new DeviceInfo(
            drive.Id.ToString(),
            drive.Label,
            drive.DevicePath,
            drive.MountPoint,
            Manufacturer: "Desconhecido",
            Model: drive.Label,
            Serial: null,
            Interface: drive.DevicePath.StartsWith("/dev/sd", StringComparison.OrdinalIgnoreCase) ? "USB/SATA" : "Desconhecida",
            drive.FileSystem,
            drive.Capacity,
            drive.UsedSpace,
            drive.FreeSpace,
            status,
            isCompatible,
            DateTimeOffset.UtcNow);
    }

    private static DeviceStatus GetStatus(UsbDrive drive, bool isCompatible)
    {
        if (!drive.IsMounted)
        {
            return DeviceStatus.Unmounted;
        }

        if (!isCompatible)
        {
            return DeviceStatus.Incompatible;
        }

        return drive.IsWritable ? DeviceStatus.Available : DeviceStatus.ReadOnly;
    }

    private static bool IsCompatibleFileSystem(UsbDriveFileSystem fileSystem)
    {
        return fileSystem is UsbDriveFileSystem.Wbfs
            or UsbDriveFileSystem.Fat32
            or UsbDriveFileSystem.Ntfs
            or UsbDriveFileSystem.ExFat;
    }
}
