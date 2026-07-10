using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Database;

namespace WiiStudio.Infrastructure.Tests.Database;

public sealed class SqliteLibraryRepositoryTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"wiistudio-sqlite-{Guid.NewGuid():N}");

    [Fact]
    public async Task UpsertGamesAsync_ShouldPersistGamesAndLibraryFiles()
    {
        var repository = CreateGameRepository();
        var game = new GameDto("RMCP01", "Mario Kart Wii", Region.Pal, DiscType.Wii, FileFormat.Wbfs, "/games/RMCP01.wbfs", 1024, DateTimeOffset.UtcNow);

        await repository.UpsertGamesAsync([game], CancellationToken.None);

        var games = await repository.GetGamesAsync(CancellationToken.None);
        games.Should().ContainSingle().Which.Id.Should().Be("RMCP01");
    }

    [Fact]
    public async Task MetadataRepository_ShouldPersistMetadata()
    {
        var unitOfWork = CreateUnitOfWork();
        var repository = new SqliteGameMetadataRepository(unitOfWork);
        var metadata = new GameMetadataDto("RMCP01", "Mario Kart Wii", Region.Pal, Developer: "Nintendo", Genres: ["Corrida"]);

        await repository.UpsertAsync(metadata, CancellationToken.None);

        var loaded = await repository.GetAsync("rmcp01", CancellationToken.None);
        loaded.Should().NotBeNull();
        loaded!.Developer.Should().Be("Nintendo");
        loaded.Genres.Should().Contain("Corrida");
    }

    [Fact]
    public async Task AssetRepository_ShouldPersistAssetsByGame()
    {
        var unitOfWork = CreateUnitOfWork();
        var repository = new SqliteGameAssetRepository(unitOfWork);
        var asset = new AssetInfo(new AssetKey("RMCP01", CoverAssetKind.FrontCover), "/covers/RMCP01.png", 2048, DateTimeOffset.UtcNow, CoverAssetSource.Cache);

        await repository.UpsertAsync(asset, CancellationToken.None);

        var assets = await repository.GetByGameIdAsync("RMCP01", CancellationToken.None);
        assets.Should().ContainSingle().Which.LocalPath.Should().Be("/covers/RMCP01.png");
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private SqliteGameLibraryRepository CreateGameRepository()
    {
        return new SqliteGameLibraryRepository(CreateUnitOfWork());
    }

    private SqliteLibraryUnitOfWork CreateUnitOfWork()
    {
        return new SqliteLibraryUnitOfWork(new LibraryDatabaseOptions
        {
            DatabasePath = Path.Combine(root, "wiistudio.db"),
            BackupBeforeMigration = false
        });
    }
}
