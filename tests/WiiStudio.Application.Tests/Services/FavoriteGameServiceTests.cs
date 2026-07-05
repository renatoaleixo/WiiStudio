using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class FavoriteGameServiceTests
{
    [Fact]
    public async Task AddAsync_ShouldMarkGameAsFavorite()
    {
        var service = new FavoriteGameService(new FakeGameLibraryService([]), new FakeFavoriteGameStore());

        await service.AddAsync("RMCP01", CancellationToken.None);

        var isFavorite = await service.IsFavoriteAsync("rmcp01", CancellationToken.None);
        isFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task AddRangeAsync_ShouldIgnoreDuplicates()
    {
        var service = new FavoriteGameService(new FakeGameLibraryService([]), new FakeFavoriteGameStore());

        await service.AddRangeAsync(["RMCP01", "RMCP01"], CancellationToken.None);

        var isFavorite = await service.IsFavoriteAsync("RMCP01", CancellationToken.None);
        isFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_ShouldUnmarkFavorite()
    {
        var service = new FavoriteGameService(new FakeGameLibraryService([]), new FakeFavoriteGameStore());

        await service.AddAsync("RMCP01", CancellationToken.None);
        await service.RemoveAsync("RMCP01", CancellationToken.None);

        var isFavorite = await service.IsFavoriteAsync("RMCP01", CancellationToken.None);
        isFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task GetFavoritesAsync_ShouldReturnFavoriteGamesFromLibrary()
    {
        var mario = CreateGame("RMCP01", "Mario Kart Wii");
        var zelda = CreateGame("RZDP01", "The Legend of Zelda");
        var service = new FavoriteGameService(new FakeGameLibraryService([mario, zelda]), new FakeFavoriteGameStore());

        await service.AddAsync("RZDP01", CancellationToken.None);
        var favorites = await service.GetFavoritesAsync(CancellationToken.None);

        var favorite = favorites.Should().ContainSingle().Subject;
        favorite.Id.Should().Be("RZDP01");
        favorite.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_ShouldRejectInvalidGameId()
    {
        var service = new FavoriteGameService(new FakeGameLibraryService([]), new FakeFavoriteGameStore());

        var act = () => service.AddAsync("invalid", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistFavorites()
    {
        var store = new FakeFavoriteGameStore();
        var service = new FavoriteGameService(new FakeGameLibraryService([]), store);

        await service.AddAsync("RMCP01", CancellationToken.None);

        store.SavedGameIds.Should().ContainSingle().Which.Should().Be("RMCP01");
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

    private sealed class FakeFavoriteGameStore : IFavoriteGameStore
    {
        private readonly IReadOnlySet<string> _loadedGameIds;

        public FakeFavoriteGameStore(IReadOnlySet<string>? loadedGameIds = null)
        {
            _loadedGameIds = loadedGameIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlySet<string> SavedGameIds { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public Task<IReadOnlySet<string>> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_loadedGameIds);
        }

        public Task SaveAsync(IReadOnlySet<string> gameIds, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SavedGameIds = gameIds;

            return Task.CompletedTask;
        }
    }
}
