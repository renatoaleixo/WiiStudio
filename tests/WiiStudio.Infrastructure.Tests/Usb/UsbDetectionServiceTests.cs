using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Usb;

namespace WiiStudio.Infrastructure.Tests.Usb;

public sealed class UsbDetectionServiceTests
{
    [Fact]
    public async Task ScanAsync_ShouldMapProviderDrivesToDtos()
    {
        var driveId = Guid.NewGuid();
        var provider = new FakeUsbProvider([
            new UsbDrive(
                driveId,
                "/dev/sdb1",
                "/media/wii",
                "Wii HD",
                UsbDriveFileSystem.Wbfs,
                1000,
                600,
                400,
                isMounted: true,
                isWritable: true)
        ]);
        var service = new UsbDetectionService(provider, NullLogger<UsbDetectionService>.Instance);

        var drives = await service.ScanAsync(CancellationToken.None);

        var drive = drives.Should().ContainSingle().Subject;
        drive.Id.Should().Be(driveId);
        drive.MountPoint.Should().Be("/media/wii");
        drive.FileSystem.Should().Be(UsbDriveFileSystem.Wbfs);
    }

    [Fact]
    public async Task UsbDriveService_ShouldDelegateDetectionToProvider()
    {
        var expectedDrive = new UsbDrive("Wii HD", "/media/wii", UsbDriveFileSystem.Wbfs, 1000, 400);
        var provider = new FakeUsbProvider([expectedDrive]);
        var service = new UsbDriveService(provider);

        var drives = await service.DetectAsync(CancellationToken.None);

        drives.Should().ContainSingle().Which.Should().BeSameAs(expectedDrive);
    }

    private sealed class FakeUsbProvider : IUsbProvider
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbProvider(IReadOnlyList<UsbDrive> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_drives);
        }
    }
}
