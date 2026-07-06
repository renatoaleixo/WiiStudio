using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class LibraryViewServiceTests
{
    [Fact]
    public async Task RefreshAsync_ShouldLoadGamesUsingSearchRequest()
    {
        var expectedGames = new[]
        {
            CreateGame("RMCP01", "Mario Kart Wii"),
            CreateGame("RSBP01", "Super Smash Bros. Brawl")
        };
        var searchService = new FakeGameSearchService(expectedGames);
        var service = new LibraryViewService(searchService);

        var snapshot = await service.RefreshAsync(new SearchRequest(SearchText: "mario"), CancellationToken.None);

        snapshot.Games.Should().BeEquivalentTo(expectedGames);
        snapshot.TotalCount.Should().Be(2);
        snapshot.State.Should().Be(LibraryViewState.Searching);
        snapshot.ViewMode.Should().Be(LibraryViewMode.Detailed);
        searchService.LastRequest!.SearchText.Should().Be("mario");
    }

    [Fact]
    public async Task ChangeViewModeAsync_ShouldPersistModeAcrossRefreshes()
    {
        var searchService = new FakeGameSearchService([CreateGame("RMCP01", "Mario Kart Wii")]);
        var service = new LibraryViewService(searchService);

        await service.ChangeViewModeAsync(LibraryViewMode.Compact, CancellationToken.None);
        var snapshot = await service.RefreshAsync(new SearchRequest(), CancellationToken.None);

        snapshot.ViewMode.Should().Be(LibraryViewMode.Compact);
        snapshot.State.Should().Be(LibraryViewState.Ready);
    }

    [Fact]
    public async Task SelectGamesAsync_ShouldKeepOnlyVisibleGames()
    {
        var searchService = new FakeGameSearchService([CreateGame("RMCP01", "Mario Kart Wii")]);
        var service = new LibraryViewService(searchService);
        await service.RefreshAsync(new SearchRequest(), CancellationToken.None);

        var snapshot = await service.SelectGamesAsync(["RMCP01", "MISSING"], CancellationToken.None);

        snapshot.SelectedGameIds.Should().BeEquivalentTo(["RMCP01"]);
        snapshot.State.Should().Be(LibraryViewState.Selecting);
    }

    private static GameDto CreateGame(string id, string title)
    {
        return new GameDto(
            id,
            title,
            Region.Pal,
            DiscType.Wii,
            FileFormat.Wbfs,
            $"/games/{id}.wbfs",
            1024,
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeGameSearchService : IGameSearchService
    {
        private readonly IReadOnlyList<GameDto> _games;

        public FakeGameSearchService(IReadOnlyList<GameDto> games)
        {
            _games = games;
        }

        public SearchRequest? LastRequest { get; private set; }

        public Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;

            return Task.FromResult(new SearchResult(_games, _games.Count, DateTimeOffset.UtcNow));
        }
    }
}
