using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Usb;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Usb;

public sealed class UsbDetectionServiceTests
{
    [Fact]
    public async Task ScanAsync_ShouldMapProviderDrivesToDtos()
    {
        var driveId = Guid.NewGuid();
        var provider = new FakeUsbDriveService([
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
        var provider = new FakeUsbDeviceProvider([
            new UsbDeviceInfo(
                expectedDrive.Id.ToString(),
                expectedDrive.Label,
                expectedDrive.DevicePath,
                expectedDrive.MountPoint,
                IsUsb: true,
                IsRemovable: true,
                expectedDrive.Capacity,
                [new UsbPartitionInfo(expectedDrive.Id.ToString(), expectedDrive.DevicePath, expectedDrive.MountPoint, "Wbfs", expectedDrive.Capacity, expectedDrive.FreeSpace, IsWbfsCandidate: true, IsWritable: true)])
        ]);
        var service = new UsbDriveService(provider);

        var drives = await service.DetectAsync(CancellationToken.None);

        var drive = drives.Should().ContainSingle().Subject;
        drive.DevicePath.Should().Be(expectedDrive.DevicePath);
        drive.MountPoint.Should().Be(expectedDrive.MountPoint);
        drive.FileSystem.Should().Be(UsbDriveFileSystem.Wbfs);
    }

    [Fact]
    public async Task LinuxUsbProvider_ShouldDetectRawWbfsPartitionFromLsblkAndWwt()
    {
        var processRunner = new FakeProcessRunner(
            """
            PATH="/dev/sdc" TYPE="disk" FSTYPE="" LABEL="" SIZE="250059350016" FSAVAIL="" MOUNTPOINT="" RO="0" MODEL="WDC WD2500BEVS-22UST0"
            PATH="/dev/sdc1" TYPE="part" FSTYPE="" LABEL="" SIZE="250059350016" FSAVAIL="" MOUNTPOINT="" RO="0" MODEL=""
            """,
            """
            type  wbfs d.usage    size  file (sizes in MiB)
            -----------------------------------------------
            BLOCK WBFS       0  238473  /dev/sdc1
            Total: 12/500 discs, 28028 MiB ~ 27 GiB used, 210440 MiB ~ 206 GiB free.
            """);
        var provider = new LinuxUsbProvider(processRunner, new FakeWiimmsToolLocator("/usr/bin/wwt"));

        var drives = await provider.DetectAsync(CancellationToken.None);

        var drive = drives.Should().ContainSingle(item => item.DevicePath == "/dev/sdc1").Subject;
        drive.FileSystem.Should().Be(UsbDriveFileSystem.Wbfs);
        drive.MountPoint.Should().Be("/dev/sdc1");
        drive.FreeSpace.Should().Be(210440L * 1024L * 1024L);
        drive.IsWritable.Should().BeTrue();
    }

    [Fact]
    public async Task LinuxUsbProvider_ShouldKeepRawWbfsPartitionWritableWhenWwtReportsPermissionDenied()
    {
        var processRunner = new FakeProcessRunner(
            """
            PATH="/dev/sdc1" TYPE="part" FSTYPE="" LABEL="" SIZE="250059350016" FSAVAIL="" MOUNTPOINT="" RO="0" MODEL=""
            """,
            """
            !! wwt: ERROR #76 [CAN'T OPEN FILE]
            !!      Can't open file: /dev/sdc1
            !!      -> Permission denied [13]
            type  wbfs d.usage    size  file (sizes in MiB)
            -----------------------------------------------
            BLOCK WBFS       0  238473  /dev/sdc1
            """);
        var provider = new LinuxUsbProvider(processRunner, new FakeWiimmsToolLocator("/usr/bin/wwt"));

        var drives = await provider.DetectAsync(CancellationToken.None);

        drives.Should().ContainSingle(item => item.DevicePath == "/dev/sdc1")
            .Which.IsWritable.Should().BeTrue();
    }

    [Fact]
    public async Task LinuxUsbProvider_ShouldKeepPermissionDeniedRawCandidateVisibleWithoutElevatedProbe()
    {
        var processRunner = new FakeProcessRunner(
            """
            PATH="/dev/sdc1" TYPE="part" FSTYPE="" LABEL="" SIZE="250059350016" FSAVAIL="" MOUNTPOINT="" RO="0" MODEL=""
            """,
            """
            !! wwt: ERROR #76 [CAN'T OPEN FILE]
            !!      Can't open file: /dev/sdc1
            !!      -> Permission denied [13]

            type  wbfs d.usage    size  file (sizes in MiB)
            -----------------------------------------------
            -      --        0       0  /dev/sdc1
            """);
        var provider = new LinuxUsbProvider(processRunner, new FakeWiimmsToolLocator("/usr/bin/wwt"));

        var drives = await provider.DetectAsync(CancellationToken.None);

        var drive = drives.Should().ContainSingle(item => item.DevicePath == "/dev/sdc1").Subject;
        drive.FileSystem.Should().Be(UsbDriveFileSystem.Wbfs);
        drive.IsWritable.Should().BeTrue();
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

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_drives);
        }
    }

    private sealed class FakeUsbDeviceProvider : IUsbDeviceProvider
    {
        private readonly IReadOnlyList<UsbDeviceInfo> _devices;

        public FakeUsbDeviceProvider(IReadOnlyList<UsbDeviceInfo> devices)
        {
            _devices = devices;
        }

        public Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_devices);
        }
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        private readonly string lsblkOutput;
        private readonly string wwtOutput;
        private readonly string? elevatedWwtOutput;

        public FakeProcessRunner(string lsblkOutput, string wwtOutput, string? elevatedWwtOutput = null)
        {
            this.lsblkOutput = lsblkOutput;
            this.wwtOutput = wwtOutput;
            this.elevatedWwtOutput = elevatedWwtOutput;
        }

        public List<ProcessRequest> Requests { get; } = [];

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            var output = request.ExecutablePath.EndsWith("lsblk", StringComparison.OrdinalIgnoreCase)
                ? lsblkOutput
                : request.ExecutablePath.EndsWith("pkexec", StringComparison.OrdinalIgnoreCase) && elevatedWwtOutput is not null
                    ? elevatedWwtOutput
                    : wwtOutput;

            return Task.FromResult(new ProcessResult(0, output, string.Empty, TimeSpan.FromMilliseconds(1)));
        }
    }

    private sealed class FakeWiimmsToolLocator : IWiimmsToolLocator
    {
        private readonly string wwtPath;

        public FakeWiimmsToolLocator(string wwtPath)
        {
            this.wwtPath = wwtPath;
        }

        public Task<string> FindWitAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult("/usr/bin/wit");
        }

        public Task<string> FindWwtAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(wwtPath);
        }
    }
}
