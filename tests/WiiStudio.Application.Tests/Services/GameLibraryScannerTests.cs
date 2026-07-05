using FluentAssertions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Application.Tests.Services;

public sealed class GameLibraryScannerTests
{
    [Fact]
    public async Task ScanAsync_ShouldReturnMappedGamesOrderedByTitle()
    {
        var drive = CreateDrive();
        var witService = new FakeWitService([
            CreateGame("RMCP01", "Mario Kart Wii"),
            CreateGame("RSBP01", "Super Smash Bros. Brawl")
        ]);
        var scanner = new GameLibraryScanner(witService);

        var result = await scanner.ScanAsync(drive, CancellationToken.None);

        result.TotalGames.Should().Be(2);
        result.Games.Select(game => game.Title).Should().Equal("Mario Kart Wii", "Super Smash Bros. Brawl");
        result.DuplicateGameIds.Should().BeEmpty();
        result.ScannedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ScanAsync_ShouldRemoveDuplicatesAndReportDuplicateGameIds()
    {
        var drive = CreateDrive();
        var witService = new FakeWitService([
            CreateGame("RMCP01", "Mario Kart Wii"),
            CreateGame("rmcp01", "Mario Kart Wii Duplicate")
        ]);
        var scanner = new GameLibraryScanner(witService);

        var result = await scanner.ScanAsync(drive, CancellationToken.None);

        result.Games.Should().ContainSingle();
        result.DuplicateGameIds.Should().ContainSingle().Which.Should().Be("RMCP01");
        result.HasDuplicates.Should().BeTrue();
    }

    [Fact]
    public async Task ScanAsync_ShouldHonorCancellation()
    {
        var drive = CreateDrive();
        var witService = new FakeWitService([]);
        var scanner = new GameLibraryScanner(witService);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var act = () => scanner.ScanAsync(drive, cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ScanGameLibraryUseCase_ShouldDelegateToScanner()
    {
        var drive = CreateDrive();
        var expectedResult = new WiiStudio.Application.DTOs.LibraryScanResult([], [], DateTimeOffset.UtcNow);
        var scanner = new FakeGameLibraryScanner(expectedResult);
        var useCase = new ScanGameLibraryUseCase(scanner);

        var result = await useCase.ExecuteAsync(drive, CancellationToken.None);

        result.Should().BeSameAs(expectedResult);
    }

    private static UsbDrive CreateDrive()
    {
        return new UsbDrive("Wii HD", "/media/wii", UsbDriveFileSystem.Wbfs, 1000, 500);
    }

    private static Game CreateGame(string gameId, string title)
    {
        return new Game(new GameId(gameId), title, FileFormat.Wbfs, 1024);
    }

    private sealed class FakeWitService : IWitService
    {
        private readonly IReadOnlyList<Game> _games;

        public FakeWitService(IReadOnlyList<Game> games)
        {
            _games = games;
        }

        public Task<Version> GetVersionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Version(3, 5));
        }

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_games);
        }
    }

    private sealed class FakeGameLibraryScanner : IGameLibraryScanner
    {
        private readonly WiiStudio.Application.DTOs.LibraryScanResult _result;

        public FakeGameLibraryScanner(WiiStudio.Application.DTOs.LibraryScanResult result)
        {
            _result = result;
        }

        public Task<WiiStudio.Application.DTOs.LibraryScanResult> ScanAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }
}
