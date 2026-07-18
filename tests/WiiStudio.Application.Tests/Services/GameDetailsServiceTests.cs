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
            new FakeFavoriteGameService([]),
            new FakeMetadataCacheService(),
            new FakeGameAssetRepository());

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
            new FakeFavoriteGameService(["RMCP01"]),
            new FakeMetadataCacheService(),
            new FakeGameAssetRepository());

        var details = await service.GetAsync("rmcp01", CancellationToken.None);

        details.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenGameIsMissing()
    {
        var service = new GameDetailsService(
            new FakeGameLibraryService([]),
            new FakeFavoriteGameService([]),
            new FakeMetadataCacheService(),
            new FakeGameAssetRepository());

        var act = () => service.GetAsync("RMCP01", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<GameNotFoundException>();
        exception.Which.GameId.Should().Be("RMCP01");
    }

    [Fact]
    public async Task GetAsync_ShouldRejectInvalidGameId()
    {
        var service = new GameDetailsService(
            new FakeGameLibraryService([]),
            new FakeFavoriteGameService([]),
            new FakeMetadataCacheService(),
            new FakeGameAssetRepository());

        var act = () => service.GetAsync("invalid", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RefreshAsync_ShouldValidateGameExists()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii");
        var service = new GameDetailsService(
            new FakeGameLibraryService([game]),
            new FakeFavoriteGameService([]),
            new FakeMetadataCacheService(),
            new FakeGameAssetRepository());

        var act = () => service.RefreshAsync("RMCP01", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnMetadataAndAssetGallery()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii");
        var metadataCache = new FakeMetadataCacheService
        {
            Metadata = new GameMetadataDto("RMCP01", "Mario Kart Wii", Region.Pal, Developer: "Nintendo")
        };
        var assets = new FakeGameAssetRepository();
        assets.Assets.Add(new AssetInfo(new AssetKey("RMCP01", CoverAssetKind.FrontCover), "/tmp/RMCP01.png", 128, DateTimeOffset.UtcNow, CoverAssetSource.Cache));
        var service = new GameDetailsService(
            new FakeGameLibraryService([game]),
            new FakeFavoriteGameService([]),
            metadataCache,
            assets);

        var details = await service.GetAsync("RMCP01", CancellationToken.None);

        details.Metadata!.Developer.Should().Be("Nintendo");
        details.Assets.Should().NotBeNull();
        details.Assets!.Should().Contain(asset => asset.Kind == CoverAssetKind.FrontCover);
    }

    [Fact]
    public async Task GetAsync_ShouldIncludeRecentImportHistory()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii");
        var history = new FakeImportHistoryRepository();
        history.Entries.Add(new ImportHistoryEntry(Guid.NewGuid(), "RMCP01", "/games/RMCP01.iso", "usb-1", "usb-write", true, "usbWrite.completed", DateTimeOffset.UtcNow));
        var service = new GameDetailsService(
            new FakeGameLibraryService([game]),
            new FakeFavoriteGameService([]),
            new FakeMetadataCacheService(),
            new FakeGameAssetRepository(),
            history);

        var details = await service.GetAsync("RMCP01", CancellationToken.None);

        details.History.Should().ContainSingle(item => item.Operation == "usb-write" && item.Success);
    }

    [Fact]
    public async Task GetAsync_ShouldExplainUnavailableFileActions()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii") with { FilePath = "/missing/RMCP01.wbfs" };
        var service = new GameDetailsService(
            new FakeGameLibraryService([game]),
            new FakeFavoriteGameService([]),
            new FakeMetadataCacheService(),
            new FakeGameAssetRepository());

        var details = await service.GetAsync("RMCP01", CancellationToken.None);

        details.FileExists.Should().BeFalse();
        details.ActionAvailability.Should().Contain(item =>
            item.Action == GameDetailsAction.Convert
            && !item.IsAvailable
            && item.UnavailableReason == "Arquivo original não encontrado.");
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

    private sealed class FakeMetadataCacheService : IMetadataCacheService
    {
        public GameMetadataDto? Metadata { get; init; }

        public Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Metadata);
        }

        public Task SaveAsync(GameMetadataDto metadata, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task InvalidateAsync(string gameId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<MetadataCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new MetadataCacheStatistics(Metadata is null ? 0 : 1, 0, Metadata?.UpdatedAt));
        }

        public Task CleanupAsync(MetadataCleanupOptions options, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeGameAssetRepository : IGameAssetRepository
    {
        public List<AssetInfo> Assets { get; } = [];

        public Task UpsertAsync(AssetInfo asset, CancellationToken cancellationToken)
        {
            Assets.Add(asset);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(AssetKey key, CancellationToken cancellationToken)
        {
            Assets.RemoveAll(asset => asset.Key == key);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AssetInfo>> GetByGameIdAsync(string gameId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AssetInfo>>(Assets.Where(asset => asset.Key.GameId == gameId).ToArray());
        }
    }

    private sealed class FakeImportHistoryRepository : IImportHistoryRepository
    {
        public List<ImportHistoryEntry> Entries { get; } = [];

        public Task AppendAsync(ImportHistoryEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ImportHistoryEntry>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ImportHistoryEntry>>(Entries.Take(limit).ToArray());
        }
    }
}
