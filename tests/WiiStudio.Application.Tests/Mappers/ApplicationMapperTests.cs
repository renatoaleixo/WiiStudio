using FluentAssertions;
using WiiStudio.Application.Mappers;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Application.Tests.Mappers;

public sealed class ApplicationMapperTests
{
    [Fact]
    public void GameMapper_ShouldMapDomainGameToDto()
    {
        var addedAt = DateTimeOffset.UtcNow;
        var game = new Game(
            new GameId("RMCP01"),
            "Mario Kart Wii",
            Region.Pal,
            DiscType.Wii,
            FileFormat.Wbfs,
            "/wbfs/RMCP01.wbfs",
            1024,
            addedAt);

        var dto = GameMapper.ToDto(game);

        dto.Id.Should().Be("RMCP01");
        dto.Title.Should().Be("Mario Kart Wii");
        dto.Region.Should().Be(Region.Pal);
        dto.Format.Should().Be(FileFormat.Wbfs);
        dto.FilePath.Should().Be("/wbfs/RMCP01.wbfs");
        dto.AddedAt.Should().Be(addedAt);
    }

    [Fact]
    public void UsbDriveMapper_ShouldMapDomainDriveToDto()
    {
        var driveId = Guid.NewGuid();
        var drive = new UsbDrive(
            driveId,
            "/dev/sdb1",
            "/media/wii",
            "Wii HD",
            UsbDriveFileSystem.Fat32,
            1000,
            600,
            400,
            isMounted: true,
            isWritable: true);

        var dto = UsbDriveMapper.ToDto(drive);

        dto.Id.Should().Be(driveId);
        dto.DevicePath.Should().Be("/dev/sdb1");
        dto.MountPoint.Should().Be("/media/wii");
        dto.Label.Should().Be("Wii HD");
        dto.FreeSpace.Should().Be(400);
    }

    [Fact]
    public void ConversionJobMapper_ShouldMapDomainJobToDto()
    {
        var job = new ConversionJob(
            Guid.NewGuid(),
            "/games/source.iso",
            "/games/output.wbfs",
            FileFormat.Iso,
            FileFormat.Wbfs);

        var dto = ConversionJobMapper.ToDto(job);

        dto.Id.Should().Be(job.Id);
        dto.SourcePath.Should().Be(job.SourcePath);
        dto.TargetFormat.Should().Be(FileFormat.Wbfs);
        dto.Status.Should().Be(ConversionJobStatus.Pending);
    }
}
