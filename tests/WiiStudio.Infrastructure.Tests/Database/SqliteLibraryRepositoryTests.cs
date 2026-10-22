using FluentAssertions;
using Microsoft.Data.Sqlite;
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

    [Fact]
    public async Task InitializeAsync_ShouldCreateImportHistoryAndMigrations()
    {
        var unitOfWork = CreateUnitOfWork();

        await unitOfWork.InitializeAsync(CancellationToken.None);

        await using var connection = unitOfWork.CreateConnection();
        await connection.OpenAsync();
        await using var tableCommand = connection.CreateCommand();
        tableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'ImportHistory';";
        var tableCount = (long)(await tableCommand.ExecuteScalarAsync() ?? 0L);
        tableCount.Should().Be(1);

        await using var migrationCommand = connection.CreateCommand();
        migrationCommand.CommandText = "SELECT Version FROM SchemaMigrations ORDER BY Version;";
        await using var reader = await migrationCommand.ExecuteReaderAsync();
        var versions = new List<long>();
        while (await reader.ReadAsync())
        {
            versions.Add(reader.GetInt64(0));
        }

        versions.Should().Contain([1, 2, 3, 4]);
    }

    [Fact]
    public async Task InitializeAsync_ShouldBackupExistingDatabaseBeforeMigration()
    {
        var databasePath = Path.Combine(root, "with-backup.db");
        Directory.CreateDirectory(root);
        await using (var existingConnection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString()))
        {
            await existingConnection.OpenAsync();
        }

        var unitOfWork = new SqliteLibraryUnitOfWork(new LibraryDatabaseOptions
        {
            DatabasePath = databasePath,
            BackupBeforeMigration = true
        });

        await unitOfWork.InitializeAsync(CancellationToken.None);

        File.Exists($"{databasePath}.bak").Should().BeTrue();
    }

    [Fact]
    public async Task ImportHistoryRepository_ShouldPersistRecentEntries()
    {
        var unitOfWork = CreateUnitOfWork();
        var repository = new SqliteImportHistoryRepository(unitOfWork);
        var first = new ImportHistoryEntry(
            Guid.NewGuid(),
            "RMCP01",
            "/games/RMCP01.iso",
            "usb-1",
            "usb-write",
            true,
            "usbWrite.completed",
            DateTimeOffset.UtcNow.AddMinutes(-5));
        var second = first with
        {
            Id = Guid.NewGuid(),
            GameId = "RSPE01",
            SourcePath = "/games/RSPE01.iso",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await repository.AppendAsync(first, CancellationToken.None);
        await repository.AppendAsync(second, CancellationToken.None);

        var entries = await repository.GetRecentAsync(1, CancellationToken.None);

        entries.Should().ContainSingle();
        entries[0].GameId.Should().Be("RSPE01");
        entries[0].Operation.Should().Be("usb-write");
        entries[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task EmulatorLaunchHistoryRepository_ShouldPersistAndFilterEntries()
    {
        var repository = new SqliteEmulatorLaunchHistoryRepository(CreateUnitOfWork());
        var first = new EmulatorLaunchHistoryEntry(
            Guid.NewGuid(), "RMCP01", "dolphin", "/games/RMCP01.wbfs",
            DateTimeOffset.UtcNow.AddMinutes(-2), null, "Started", null);
        var second = new EmulatorLaunchHistoryEntry(
            Guid.NewGuid(), "RSPE01", "dolphin", "/games/RSPE01.wbfs",
            DateTimeOffset.UtcNow, null, "Blocked", "Arquivo ausente");

        await repository.AppendAsync(first);
        await repository.AppendAsync(second);

        var entries = await repository.GetRecentAsync("rmcp01");

        entries.Should().ContainSingle();
        entries[0].GameId.Should().Be("RMCP01");
        entries[0].Result.Should().Be("Started");
    }

    [Fact]
    public async Task EmulatorProfileRepository_ShouldPersistDefaultAndGameAssignment()
    {
        var repository = new SqliteEmulatorProfileRepository(CreateUnitOfWork());
        var profile = new EmulatorProfileDto(
            "profile-1", "dolphin", "Padrão", true, "Teclado", true, "Vulkan", "Teste",
            new Dictionary<string, string>(), DateTimeOffset.UtcNow);

        await repository.UpsertAsync(profile);
        await repository.UpsertAssignmentAsync(new GameEmulatorProfileAssignmentDto("RMCP01", "dolphin", profile.ProfileId, DateTimeOffset.UtcNow));

        var loaded = await repository.GetByIdAsync(profile.ProfileId);
        var assignment = await repository.GetAssignmentAsync("rmcp01", "dolphin");
        loaded.Should().BeEquivalentTo(profile, options => options.Excluding(item => item.UpdatedAt));
        assignment.Should().NotBeNull();
        assignment!.ProfileId.Should().Be(profile.ProfileId);
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
