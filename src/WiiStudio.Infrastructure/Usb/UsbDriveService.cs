using WiiStudio.Application.Interfaces;
using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Infrastructure.Usb;

public sealed class UsbDriveService : IUsbDriveService
{
    private readonly IUsbDeviceProvider _usbDeviceProvider;

    public UsbDriveService(IUsbDeviceProvider usbDeviceProvider)
    {
        _usbDeviceProvider = usbDeviceProvider;
    }

    public async Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
    {
        var devices = await _usbDeviceProvider.GetDevicesAsync(cancellationToken);
        return devices
            .SelectMany(MapDevice)
            .GroupBy(drive => drive.DevicePath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    private static IEnumerable<UsbDrive> MapDevice(UsbDeviceInfo device)
    {
        if (device.Partitions.Count == 0)
        {
            yield break;
        }

        foreach (var partition in device.Partitions)
        {
            var path = partition.DevicePath ?? partition.MountPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            var fileSystem = MapFileSystem(partition.FileSystem);
            if (!IsCompatibleFileSystem(fileSystem) && !partition.IsWbfsCandidate)
            {
                continue;
            }

            var capacity = partition.SizeBytes ?? device.SizeBytes ?? 0;
            var free = partition.FreeBytes ?? 0;
            var used = Math.Max(0, capacity - free);
            yield return new UsbDrive(
                CreateStableId(partition.PartitionId.Length == 0 ? path : partition.PartitionId),
                path,
                partition.MountPath ?? path,
                device.DisplayName,
                partition.IsWbfsCandidate ? UsbDriveFileSystem.Wbfs : fileSystem,
                capacity,
                used,
                free,
                isMounted: !string.IsNullOrWhiteSpace(partition.MountPath) || partition.IsWbfsCandidate,
                partition.IsWritable);
        }
    }

    private static UsbDriveFileSystem MapFileSystem(string? fileSystem)
    {
        return fileSystem?.ToUpperInvariant() switch
        {
            "WBFS" => UsbDriveFileSystem.Wbfs,
            "FAT32" => UsbDriveFileSystem.Fat32,
            "NTFS" => UsbDriveFileSystem.Ntfs,
            "EXFAT" => UsbDriveFileSystem.ExFat,
            _ => UsbDriveFileSystem.Unknown
        };
    }

    private static bool IsCompatibleFileSystem(UsbDriveFileSystem fileSystem)
    {
        return fileSystem is UsbDriveFileSystem.Wbfs
            or UsbDriveFileSystem.Fat32
            or UsbDriveFileSystem.Ntfs
            or UsbDriveFileSystem.ExFat;
    }

    private static Guid CreateStableId(string value)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value.Trim().ToUpperInvariant()));
        return new Guid(hash[..16]);
    }
}
