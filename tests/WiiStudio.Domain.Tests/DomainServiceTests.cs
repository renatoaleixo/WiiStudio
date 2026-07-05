using FluentAssertions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Domain.Services;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Tests;

public sealed class DomainServiceTests
{
    [Fact]
    public void DuplicateGameService_ShouldDetectDuplicateGameId()
    {
        var service = new DuplicateGameService();
        var game = new Game(new GameId("RMCP01"), "Mario Kart Wii", FileFormat.Wbfs, 1024);

        var containsDuplicate = service.ContainsDuplicate([game], new GameId("rmcp01"));

        containsDuplicate.Should().BeTrue();
    }

    [Fact]
    public void UsbCapacityService_ShouldValidateAvailableSpace()
    {
        var service = new UsbCapacityService();
        var drive = new UsbDrive("Wii HD", "/media/wii", UsbDriveFileSystem.Wbfs, 1000, 400);

        service.HasEnoughSpace(drive, 399).Should().BeTrue();
        service.HasEnoughSpace(drive, 401).Should().BeFalse();
    }
}
