using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Usb;

namespace WiiStudio.Infrastructure.Tests.Usb;

public sealed class CrossPlatformUsbDeviceProviderTests
{
    [Fact]
    public async Task GetDevicesAsync_ShouldSelectProviderForCurrentPlatform()
    {
        var linuxProvider = new FakePlatformProvider(OSPlatformKind.Linux, "linux-device");
        var windowsProvider = new FakePlatformProvider(OSPlatformKind.Windows, "windows-device");
        var provider = new CrossPlatformUsbDeviceProvider(
            new FakeOperatingSystemInfo(OSPlatformKind.Windows),
            [linuxProvider, windowsProvider],
            NullLogger<CrossPlatformUsbDeviceProvider>.Instance);

        var devices = await provider.GetDevicesAsync(CancellationToken.None);

        devices.Should().ContainSingle(device => device.DeviceId == "windows-device");
    }

    [Fact]
    public async Task GetDevicesAsync_ShouldReturnEmptyWhenProviderThrows()
    {
        var provider = new CrossPlatformUsbDeviceProvider(
            new FakeOperatingSystemInfo(OSPlatformKind.Linux),
            [new ThrowingPlatformProvider(OSPlatformKind.Linux)],
            NullLogger<CrossPlatformUsbDeviceProvider>.Instance);

        var devices = await provider.GetDevicesAsync(CancellationToken.None);

        devices.Should().BeEmpty();
    }

    private sealed class FakeOperatingSystemInfo : IOperatingSystemInfo
    {
        public FakeOperatingSystemInfo(OSPlatformKind current)
        {
            Current = current;
        }

        public OSPlatformKind Current { get; }
    }

    private sealed class FakePlatformProvider : IPlatformUsbDeviceProvider
    {
        private readonly string _deviceId;

        public FakePlatformProvider(OSPlatformKind platform, string deviceId)
        {
            Platform = platform;
            _deviceId = deviceId;
        }

        public OSPlatformKind Platform { get; }

        public Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<UsbDeviceInfo> devices =
            [
                new(_deviceId, _deviceId, null, null, IsUsb: true, IsRemovable: true, SizeBytes: null, Partitions: [])
            ];
            return Task.FromResult(devices);
        }
    }

    private sealed class ThrowingPlatformProvider : IPlatformUsbDeviceProvider
    {
        public ThrowingPlatformProvider(OSPlatformKind platform)
        {
            Platform = platform;
        }

        public OSPlatformKind Platform { get; }

        public Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("boom");
        }
    }
}
