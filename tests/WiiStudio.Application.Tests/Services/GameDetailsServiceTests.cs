using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class GameDetailsServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnGameDetailsFromLibrary()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii");
        var service = new GameDetailsService(
            new FakeGameLibraryService([game]),
            new FakeFavoriteGameService([]));

        var details = await service.GetAsync("RMCP01", CancellationToken.None);

        details.Id.Should().Be("RMCP01");
        details.Title.Should().Be("Mario Kart Wii");
        details.Format.Should().Be(FileFormat.Wbfs);
        details.AvailableActions.Should().Contain(GameDetailsAction.Remove);
        details.AvailableActions.Should().Contain(GameDetailsAction.ToggleFavorite);
    }

    [Fact]
    public async Task GetAsync_ShouldMarkFavoriteGames()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii");
        var service = new GameDetailsService(
            new FakeGameLibraryService([game]),
            new FakeFavoriteGameService(["RMCP01"]));

        var details = await service.GetAsync("rmcp01", CancellationToken.None);

        details.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenGameIsMissing()
    {
        var service = new GameDetailsService(
            new FakeGameLibraryService([]),
            new FakeFavoriteGameService([]));

        var act = () => service.GetAsync("RMCP01", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GameNotFoundException>();
        exception.Which.GameId.Should().Be("RMCP01");
    }

    [Fact]
    public async Task GetAsync_ShouldRejectInvalidGameId()
    {
        var service = new GameDetailsService(
            new FakeGameLibraryService([]),
            new FakeFavoriteGameService([]));

        var act = () => service.GetAsync("invalid", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RefreshAsync_ShouldValidateGameExists()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii");
        var service = new GameDetailsService(
            new FakeGameLibraryService([game]),
            new FakeFavoriteGameService([]));

        var act = () => service.RefreshAsync("RMCP01", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private static GameDto CreateGame(string id, string title)
    {
        return new GameDto(id, title, Region.Pal, DiscType.Wii, FileFormat.Wbfs, $"/games/{id}.wbfs", 1024, DateTimeOffset.UtcNow);
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

    private sealed class FakeFavoriteGameService : IFavoriteGameService
    {
        private readonly HashSet<string> _favoriteGameIds;

        public FakeFavoriteGameService(IReadOnlyList<string> favoriteGameIds)
        {
            _favoriteGameIds = new HashSet<string>(favoriteGameIds, StringComparer.OrdinalIgnoreCase);
        }

        public Task AddAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _favoriteGameIds.Add(gameId);

            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var gameId in gameIds)
            {
                _favoriteGameIds.Add(gameId);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _favoriteGameIds.Remove(gameId);

            return Task.CompletedTask;
        }

        public Task<bool> IsFavoriteAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_favoriteGameIds.Contains(gameId));
        }

        public Task<IReadOnlyList<GameDto>> GetFavoritesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<GameDto>>([]);
        }
    }
}
