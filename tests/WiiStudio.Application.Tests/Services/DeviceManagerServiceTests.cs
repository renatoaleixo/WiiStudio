using FluentAssertions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class DeviceManagerServiceTests
{
    [Fact]
    public async Task RefreshAsync_ShouldMapDetectedDrivesToDeviceInfo()
    {
        var drive = CreateDrive(fileSystem: UsbDriveFileSystem.Wbfs);
        var service = new DeviceManagerService(new FakeUsbDriveService([drive]));

        var snapshot = await service.RefreshAsync(CancellationToken.None);

        snapshot.Devices.Should().ContainSingle();
        var device = snapshot.Devices[0];
        device.DeviceId.Should().Be(drive.Id.ToString());
        device.Name.Should().Be("Wii HD");
        device.FileSystem.Should().Be(UsbDriveFileSystem.Wbfs);
        device.Status.Should().Be(WiiStudio.Application.DTOs.DeviceStatus.Available);
        device.IsWiiCompatible.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_ShouldMarkUnknownFileSystemAsIncompatible()
    {
        var drive = CreateDrive(fileSystem: UsbDriveFileSystem.Unknown);
        var service = new DeviceManagerService(new FakeUsbDriveService([drive]));

        var snapshot = await service.RefreshAsync(CancellationToken.None);

        snapshot.Devices.Should().ContainSingle().Which.Status.Should().Be(WiiStudio.Application.DTOs.DeviceStatus.Incompatible);
        snapshot.Devices[0].IsWiiCompatible.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshAsync_ShouldMarkReadOnlyAndUnmountedDevices()
    {
        var readOnly = CreateDrive(label: "Read Only", isWritable: false);
        var unmounted = CreateDrive(label: "Unmounted", isMounted: false);
        var service = new DeviceManagerService(new FakeUsbDriveService([readOnly, unmounted]));

        var snapshot = await service.RefreshAsync(CancellationToken.None);

        snapshot.Devices.Single(device => device.Name == "Read Only").Status.Should().Be(WiiStudio.Application.DTOs.DeviceStatus.ReadOnly);
        snapshot.Devices.Single(device => device.Name == "Unmounted").Status.Should().Be(WiiStudio.Application.DTOs.DeviceStatus.Unmounted);
    }

    [Fact]
    public async Task EjectAsync_ShouldMarkDeviceAsEjected()
    {
        var drive = CreateDrive();
        var service = new DeviceManagerService(new FakeUsbDriveService([drive]));
        await service.RefreshAsync(CancellationToken.None);

        var ejected = await service.EjectAsync(drive.Id.ToString(), CancellationToken.None);

        ejected.Status.Should().Be(WiiStudio.Application.DTOs.DeviceStatus.Ejected);
        var devices = await service.GetDevicesAsync(CancellationToken.None);
        devices.Should().ContainSingle().Which.Status.Should().Be(WiiStudio.Application.DTOs.DeviceStatus.Ejected);
    }

    private static UsbDrive CreateDrive(
        string label = "Wii HD",
        UsbDriveFileSystem fileSystem = UsbDriveFileSystem.Wbfs,
        bool isMounted = true,
        bool isWritable = true)
    {
        return new UsbDrive(
            Guid.NewGuid(),
            "/dev/sdb1",
            "/media/wii",
            label,
            fileSystem,
            capacity: 1000,
            usedSpace: 400,
            freeSpace: 600,
            isMounted,
            isWritable);
    }

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_drives);
        }
    }
}
