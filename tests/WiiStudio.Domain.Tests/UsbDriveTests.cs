using FluentAssertions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Domain.Tests;

public sealed class UsbDriveTests
{
    [Fact]
    public void Constructor_ShouldExposeCompatibilityAliases()
    {
        var drive = new UsbDrive("Wii HD", "/media/wii", UsbDriveFileSystem.Wbfs, 1000, 250);

        drive.Name.Should().Be("Wii HD");
        drive.Label.Should().Be("Wii HD");
        drive.MountPath.Should().Be("/media/wii");
        drive.MountPoint.Should().Be("/media/wii");
        drive.TotalSizeInBytes.Should().Be(1000);
        drive.AvailableSizeInBytes.Should().Be(250);
        drive.UsedSpace.Should().Be(750);
        drive.IsWbfs.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldRejectCapacityOverflow()
    {
        var act = () => new UsbDrive(
            Guid.NewGuid(),
            "/dev/sdb1",
            "/media/wii",
            "Wii HD",
            UsbDriveFileSystem.Fat32,
            1000,
            800,
            300,
            true,
            true);

        act.Should().Throw<ArgumentException>();
    }
}
