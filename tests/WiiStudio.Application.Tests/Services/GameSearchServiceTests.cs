using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class GameSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldFindGamesByTitleIgnoringCaseAndAccents()
    {
        var games = new[]
        {
            CreateGame("RMCP01", "Mario Kart Wii", Region.Pal, FileFormat.Wbfs, 1000),
            CreateGame("RZDP01", "The Legend of Zélda", Region.Pal, FileFormat.Iso, 2000)
        };
        var service = new GameSearchService(new FakeGameLibraryService(games));

        var result = await service.SearchAsync(new SearchRequest(SearchText: "zelda"), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Games.Should().ContainSingle().Which.Id.Should().Be("RZDP01");
    }

    [Fact]
    public async Task SearchAsync_ShouldFindGamesById()
    {
        var games = new[]
        {
            CreateGame("RMCP01", "Mario Kart Wii", Region.Pal, FileFormat.Wbfs, 1000),
            CreateGame("RSBE01", "Super Smash Bros. Brawl", Region.NtscU, FileFormat.Iso, 2000)
        };
        var service = new GameSearchService(new FakeGameLibraryService(games));

        var result = await service.SearchAsync(new SearchRequest(SearchText: "rsbe"), CancellationToken.None);

        result.Games.Should().ContainSingle().Which.Title.Should().Be("Super Smash Bros. Brawl");
    }

    [Fact]
    public async Task SearchAsync_ShouldApplyCombinedFilters()
    {
        var games = new[]
        {
            CreateGame("RMCP01", "Mario Kart Wii", Region.Pal, FileFormat.Wbfs, 1000),
            CreateGame("RSBE01", "Super Smash Bros. Brawl", Region.NtscU, FileFormat.Wbfs, 2000),
            CreateGame("RZDP01", "The Legend of Zelda", Region.Pal, FileFormat.Iso, 3000)
        };
        var service = new GameSearchService(new FakeGameLibraryService(games));

        var result = await service.SearchAsync(
            new SearchRequest(
                Regions: new HashSet<Region> { Region.Pal },
                Formats: new HashSet<FileFormat> { FileFormat.Wbfs },
                MinimumSizeInBytes: 500,
                MaximumSizeInBytes: 1500),
            CancellationToken.None);

        result.Games.Should().ContainSingle().Which.Id.Should().Be("RMCP01");
    }

    [Fact]
    public async Task SearchAsync_ShouldSortBySizeDescending()
    {
        var games = new[]
        {
            CreateGame("RMCP01", "Mario Kart Wii", Region.Pal, FileFormat.Wbfs, 1000),
            CreateGame("RSBE01", "Super Smash Bros. Brawl", Region.NtscU, FileFormat.Iso, 3000),
            CreateGame("RZDP01", "The Legend of Zelda", Region.Pal, FileFormat.Iso, 2000)
        };
        var service = new GameSearchService(new FakeGameLibraryService(games));

        var result = await service.SearchAsync(
            new SearchRequest(SortField: GameSortField.Size, SortDescending: true),
            CancellationToken.None);

        result.Games.Select(game => game.Id).Should().Equal("RSBE01", "RZDP01", "RMCP01");
    }

    private static GameDto CreateGame(string id, string title, Region region, FileFormat format, long sizeInBytes)
    {
        return new GameDto(id, title, region, DiscType.Wii, format, $"/games/{id}.wbfs", sizeInBytes, DateTimeOffset.UtcNow);
    }

    private sealed class FakeGameLibraryService : IGameLibraryService
    {
        private readonly IReadOnlyList<GameDto> _games;

        public FakeGameLibraryService(IReadOnlyList<GameDto> games)
        {
            _games = games;
        }

        public Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_games);
        }

        public Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
