using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class GameLibraryServiceTests
{
    [Fact]
    public async Task RefreshAsync_ShouldScanDriveAndCacheGames()
    {
        var drive = CreateDrive();
        var game = CreateGameDto("RMCP01", "Mario Kart Wii");
        var service = new GameLibraryService(
            new FakeUsbDriveService([drive]),
            new FakeGameLibraryScanner(new LibraryScanResult([game], [], DateTimeOffset.UtcNow)));

        await service.RefreshAsync(drive.Id, CancellationToken.None);
        var games = await service.GetGamesAsync(drive.Id, CancellationToken.None);

        games.Should().ContainSingle().Which.Should().Be(game);
    }

    [Fact]
    public async Task GetGamesAsync_ShouldReturnAllCachedGamesWhenDriveIsNotSpecified()
    {
        var driveA = CreateDrive();
        var driveB = CreateDrive();
        var gameA = CreateGameDto("RMCP01", "Mario Kart Wii");
        var gameB = CreateGameDto("RSBP01", "Super Smash Bros. Brawl");
        var scanner = new SequenceGameLibraryScanner(
            new LibraryScanResult([gameA], [], DateTimeOffset.UtcNow),
            new LibraryScanResult([gameB], [], DateTimeOffset.UtcNow));
        var service = new GameLibraryService(new FakeUsbDriveService([driveA, driveB]), scanner);

        await service.RefreshAsync(driveA.Id, CancellationToken.None);
        await service.RefreshAsync(driveB.Id, CancellationToken.None);
        var games = await service.GetGamesAsync(null, CancellationToken.None);

        games.Should().BeEquivalentTo([gameA, gameB]);
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrowWhenDriveIsMissing()
    {
        var missingDriveId = Guid.NewGuid();
        var service = new GameLibraryService(
            new FakeUsbDriveService([]),
            new FakeGameLibraryScanner(new LibraryScanResult([], [], DateTimeOffset.UtcNow)));

        var act = () => service.RefreshAsync(missingDriveId, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<UsbDriveNotFoundException>();
        exception.Which.UsbDriveId.Should().Be(missingDriveId);
    }

    private static UsbDrive CreateDrive()
    {
        return new UsbDrive(Guid.NewGuid(), "/dev/sdb1", "/media/wii", "Wii HD", UsbDriveFileSystem.Wbfs, 1000, 500, 500, true, true);
    }

    private static GameDto CreateGameDto(string id, string title)
    {
        return new GameDto(id, title, Region.Pal, DiscType.Wii, FileFormat.Wbfs, $"/media/wii/{id}.wbfs", 1024, DateTimeOffset.UtcNow);
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

    private sealed class FakeGameLibraryScanner : IGameLibraryScanner
    {
        private readonly LibraryScanResult _result;

        public FakeGameLibraryScanner(LibraryScanResult result)
        {
            _result = result;
        }

        public Task<LibraryScanResult> ScanAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_result);
        }
    }

    private sealed class SequenceGameLibraryScanner : IGameLibraryScanner
    {
        private readonly Queue<LibraryScanResult> _results;

        public SequenceGameLibraryScanner(params LibraryScanResult[] results)
        {
            _results = new Queue<LibraryScanResult>(results);
        }

        public Task<LibraryScanResult> ScanAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_results.Dequeue());
        }
    }
}
