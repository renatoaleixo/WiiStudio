using FluentAssertions;
using System.IO.Compression;
using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.AppUpdates;
using WiiStudio.Infrastructure.Backups;
using WiiStudio.Infrastructure.Covers;
using WiiStudio.Infrastructure.Favorites;
using WiiStudio.Infrastructure.GameTdb;
using WiiStudio.Infrastructure.Metadata;
using WiiStudio.Infrastructure.Notifications;
using WiiStudio.Infrastructure.Profiles;
using WiiStudio.Infrastructure.Settings;
using WiiStudio.Infrastructure.Tasks;

namespace WiiStudio.Infrastructure.Tests.Backups;

public sealed class ZipBackupServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldReturnExistingConfiguredItems()
    {
        using var fixture = TemporaryBackupFixture.Create();
        fixture.WriteSettings("settings");
        fixture.WriteFavoriteGames("favorites");
        var service = fixture.CreateService();

        var preview = await service.AnalyzeAsync(CancellationToken.None);

        preview.Items.Should().Contain(item => item.Kind == BackupItemKind.Settings && item.Exists);
        preview.Items.Should().Contain(item => item.Kind == BackupItemKind.Favorites && item.Exists);
        preview.TotalSizeInBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateZipWithManifestAndSelectedFiles()
    {
        using var fixture = TemporaryBackupFixture.Create();
        fixture.WriteSettings("settings");
        fixture.WriteFavoriteGames("favorites");
        var service = fixture.CreateService();
        var outputFile = Path.Combine(fixture.RootDirectory, "backup.zip");

        await service.CreateAsync(
            new BackupOptions(IncludeProfiles: false, IncludeMetadata: false, IncludeNotifications: false, IncludeTasks: false, IncludeUpdates: false),
            outputFile,
            CancellationToken.None);

        using var archive = ZipFile.OpenRead(outputFile);
        archive.GetEntry("manifest.json").Should().NotBeNull();
        archive.GetEntry("settings/settings.json").Should().NotBeNull();
        archive.GetEntry("favorites/favorites.json").Should().NotBeNull();
    }

    [Fact]
    public async Task RestoreAsync_ShouldRestoreSelectedFilesAndCreateRestorePoint()
    {
        using var source = TemporaryBackupFixture.Create();
        source.WriteSettings("source-settings");
        source.WriteFavoriteGames("source-favorites");
        var backupFile = Path.Combine(source.RootDirectory, "backup.zip");
        await source.CreateService().CreateAsync(new BackupOptions(IncludeProfiles: false, IncludeMetadata: false, IncludeNotifications: false, IncludeTasks: false, IncludeUpdates: false), backupFile, CancellationToken.None);

        using var target = TemporaryBackupFixture.Create();
        target.WriteSettings("target-settings");
        target.WriteFavoriteGames("target-favorites");
        var service = target.CreateService();

        await service.RestoreAsync(
            backupFile,
            new RestoreOptions(RestoreProfiles: false, RestoreMetadata: false, RestoreNotifications: false, RestoreTasks: false, RestoreUpdates: false),
            CancellationToken.None);

        File.ReadAllText(target.SettingsPath).Should().Be("source-settings");
        File.ReadAllText(target.FavoritesPath).Should().Be("source-favorites");
        Directory.EnumerateFiles(target.RestorePointDirectory, "*.zip").Should().ContainSingle();
    }

    [Fact]
    public async Task RestoreAsync_ShouldRestoreOnlySelectedItems()
    {
        using var source = TemporaryBackupFixture.Create();
        source.WriteSettings("source-settings");
        source.WriteFavoriteGames("source-favorites");
        var backupFile = Path.Combine(source.RootDirectory, "backup.zip");
        await source.CreateService().CreateAsync(new BackupOptions(IncludeProfiles: false, IncludeMetadata: false, IncludeNotifications: false, IncludeTasks: false, IncludeUpdates: false), backupFile, CancellationToken.None);

        using var target = TemporaryBackupFixture.Create();
        target.WriteSettings("target-settings");
        target.WriteFavoriteGames("target-favorites");

        await target.CreateService().RestoreAsync(
            backupFile,
            new RestoreOptions(RestoreFavorites: false, RestoreProfiles: false, RestoreMetadata: false, RestoreNotifications: false, RestoreTasks: false, RestoreUpdates: false),
            CancellationToken.None);

        File.ReadAllText(target.SettingsPath).Should().Be("source-settings");
        File.ReadAllText(target.FavoritesPath).Should().Be("target-favorites");
    }

    [Fact]
    public async Task RestoreAsync_ShouldRejectBackupWithInvalidChecksum()
    {
        using var fixture = TemporaryBackupFixture.Create();
        fixture.WriteSettings("settings");
        var backupFile = Path.Combine(fixture.RootDirectory, "backup.zip");
        var service = fixture.CreateService();
        await service.CreateAsync(new BackupOptions(IncludeProfiles: false, IncludeFavorites: false, IncludeMetadata: false, IncludeNotifications: false, IncludeTasks: false, IncludeUpdates: false), backupFile, CancellationToken.None);
        using (var archive = ZipFile.Open(backupFile, ZipArchiveMode.Update))
        {
            var entry = archive.GetEntry("settings/settings.json")!;
            entry.Delete();
            var replacement = archive.CreateEntry("settings/settings.json");
            await using var stream = replacement.Open();
            await JsonSerializer.SerializeAsync(stream, "tampered");
        }

        var act = () => service.RestoreAsync(
            backupFile,
            new RestoreOptions(RestoreProfiles: false, RestoreFavorites: false, RestoreMetadata: false, RestoreNotifications: false, RestoreTasks: false, RestoreUpdates: false),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private sealed class TemporaryBackupFixture : IDisposable
    {
        private TemporaryBackupFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            SettingsPath = Path.Combine(rootDirectory, "config", "settings.json");
            FavoritesPath = Path.Combine(rootDirectory, "favorites.json");
            ProfilesRoot = Path.Combine(rootDirectory, "profiles");
            MetadataPath = Path.Combine(rootDirectory, "metadata", "games.json");
            NotificationsPath = Path.Combine(rootDirectory, "notifications", "history.json");
            TasksPath = Path.Combine(rootDirectory, "tasks", "tasks.json");
            GameTdbStatePath = Path.Combine(rootDirectory, "metadata", "gametdb-state.json");
            AppUpdateStatePath = Path.Combine(rootDirectory, "updates", "app-update-state.json");
            CoverCacheRoot = Path.Combine(rootDirectory, "assets");
            RestorePointDirectory = Path.Combine(rootDirectory, "restore-points");
        }

        public string RootDirectory { get; }

        public string SettingsPath { get; }

        public string FavoritesPath { get; }

        public string ProfilesRoot { get; }

        public string MetadataPath { get; }

        public string NotificationsPath { get; }

        public string TasksPath { get; }

        public string GameTdbStatePath { get; }

        public string AppUpdateStatePath { get; }

        public string CoverCacheRoot { get; }

        public string RestorePointDirectory { get; }

        public static TemporaryBackupFixture Create()
        {
            return new TemporaryBackupFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-backup-{Guid.NewGuid():N}"));
        }

        public void WriteSettings(string content)
        {
            WriteFile(SettingsPath, content);
        }

        public void WriteFavoriteGames(string content)
        {
            WriteFile(FavoritesPath, content);
        }

        public ZipBackupService CreateService()
        {
            return new ZipBackupService(
                new BackupServiceOptions { RestorePointDirectory = RestorePointDirectory, ApplicationVersion = "1.0.0" },
                new SettingsStoreOptions { FilePath = SettingsPath },
                new ProfileStoreOptions { RootDirectory = ProfilesRoot },
                new FavoriteGameStoreOptions { FilePath = FavoritesPath },
                new MetadataCacheOptions { FilePath = MetadataPath },
                new NotificationStoreOptions { FilePath = NotificationsPath },
                new TaskManagerOptions { FilePath = TasksPath },
                new GameTdbUpdateStateStoreOptions { FilePath = GameTdbStatePath },
                new ApplicationUpdateStateStoreOptions { FilePath = AppUpdateStatePath },
                new CoverCacheOptions { RootDirectory = CoverCacheRoot });
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }

        private static void WriteFile(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content);
        }
    }
}
