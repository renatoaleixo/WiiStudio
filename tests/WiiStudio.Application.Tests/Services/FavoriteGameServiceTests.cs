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
        var service = new FavoriteGameService(new FakeGameLibraryService([]));

        await service.AddAsync("RMCP01", CancellationToken.None);

        var isFavorite = await service.IsFavoriteAsync("rmcp01", CancellationToken.None);
        isFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task AddRangeAsync_ShouldIgnoreDuplicates()
    {
        var service = new FavoriteGameService(new FakeGameLibraryService([]));

        await service.AddRangeAsync(["RMCP01", "RMCP01"], CancellationToken.None);

        var isFavorite = await service.IsFavoriteAsync("RMCP01", CancellationToken.None);
        isFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_ShouldUnmarkFavorite()
    {
        var service = new FavoriteGameService(new FakeGameLibraryService([]));

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
        var service = new FavoriteGameService(new FakeGameLibraryService([mario, zelda]));

        await service.AddAsync("RZDP01", CancellationToken.None);
        var favorites = await service.GetFavoritesAsync(CancellationToken.None);

        var favorite = favorites.Should().ContainSingle().Subject;
        favorite.Id.Should().Be("RZDP01");
        favorite.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_ShouldRejectInvalidGameId()
    {
        var service = new FavoriteGameService(new FakeGameLibraryService([]));

        var act = () => service.AddAsync("invalid", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
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
}
