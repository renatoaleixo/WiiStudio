using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.AppUpdates;
using WiiStudio.Infrastructure.Covers;
using WiiStudio.Infrastructure.Favorites;
using WiiStudio.Infrastructure.GameTdb;
using WiiStudio.Infrastructure.Metadata;
using WiiStudio.Infrastructure.Notifications;
using WiiStudio.Infrastructure.Profiles;
using WiiStudio.Infrastructure.Settings;
using WiiStudio.Infrastructure.Tasks;

namespace WiiStudio.Infrastructure.Backups;

public sealed class ZipBackupService : IBackupService
{
    private const string ManifestPath = "manifest.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BackupServiceOptions _options;
    private readonly SettingsStoreOptions _settingsOptions;
    private readonly ProfileStoreOptions _profileOptions;
    private readonly FavoriteGameStoreOptions _favoriteOptions;
    private readonly MetadataCacheOptions _metadataOptions;
    private readonly NotificationStoreOptions _notificationOptions;
    private readonly TaskManagerOptions _taskOptions;
    private readonly GameTdbUpdateStateStoreOptions _gameTdbOptions;
    private readonly ApplicationUpdateStateStoreOptions _appUpdateOptions;
    private readonly CoverCacheOptions _coverOptions;

    public ZipBackupService(
        BackupServiceOptions options,
        SettingsStoreOptions settingsOptions,
        ProfileStoreOptions profileOptions,
        FavoriteGameStoreOptions favoriteOptions,
        MetadataCacheOptions metadataOptions,
        NotificationStoreOptions notificationOptions,
        TaskManagerOptions taskOptions,
        GameTdbUpdateStateStoreOptions gameTdbOptions,
        ApplicationUpdateStateStoreOptions appUpdateOptions,
        CoverCacheOptions coverOptions)
    {
        _options = options;
        _settingsOptions = settingsOptions;
        _profileOptions = profileOptions;
        _favoriteOptions = favoriteOptions;
        _metadataOptions = metadataOptions;
        _notificationOptions = notificationOptions;
        _taskOptions = taskOptions;
        _gameTdbOptions = gameTdbOptions;
        _appUpdateOptions = appUpdateOptions;
        _coverOptions = coverOptions;
    }

    public Task<BackupPreview> AnalyzeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sources = GetSources(new BackupOptions(IncludeCache: true, IncludeLogs: true));
        var items = sources
            .SelectMany(source => GetFiles(source)
                .DefaultIfEmpty()
                .Select(filePath => CreatePreviewItem(source, filePath)))
            .ToArray();

        return Task.FromResult(new BackupPreview(items, items.Sum(item => item.SizeInBytes), DateTimeOffset.UtcNow));
    }

    public async Task CreateAsync(BackupOptions options, string outputFile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidatePath(outputFile, nameof(outputFile));
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var sources = GetSources(options)
            .Where(source => source.Exists)
            .ToArray();
        var manifestEntries = new List<BackupManifestEntry>();

        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        using var archive = ZipFile.Open(outputFile, ZipArchiveMode.Create);
        foreach (var source in sources)
        {
            foreach (var filePath in GetFiles(source))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entryPath = GetEntryPath(source, filePath);
                archive.CreateEntryFromFile(filePath, entryPath, CompressionLevel.Optimal);
                manifestEntries.Add(new BackupManifestEntry(
                    source.Kind,
                    entryPath,
                    GetFileRelativePath(source, filePath),
                    ComputeSha256(filePath),
                    new FileInfo(filePath).Length));
            }
        }

        var manifest = new BackupManifest(_options.ApplicationVersion, DateTimeOffset.UtcNow, manifestEntries);
        var manifestEntry = archive.CreateEntry(ManifestPath, CompressionLevel.Optimal);
        await using var manifestStream = manifestEntry.Open();
        await JsonSerializer.SerializeAsync(manifestStream, manifest, JsonOptions, cancellationToken);
    }

    public async Task RestoreAsync(string backupFile, RestoreOptions options, CancellationToken cancellationToken)
    {
        ValidatePath(backupFile, nameof(backupFile));
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(backupFile))
        {
            throw new FileNotFoundException("Backup file was not found.", backupFile);
        }

        await CreateRestorePointAsync(options, cancellationToken);

        using var archive = ZipFile.OpenRead(backupFile);
        var manifest = await LoadManifestAsync(archive, cancellationToken);
        foreach (var entry in manifest.Entries.Where(entry => ShouldRestore(entry.Kind, options)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var archiveEntry = archive.GetEntry(entry.EntryPath)
                ?? throw new InvalidOperationException($"Backup entry '{entry.EntryPath}' was not found.");
            await using var archiveStream = archiveEntry.Open();
            var checksum = await ComputeSha256Async(archiveStream, cancellationToken);
            if (!string.Equals(checksum, entry.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Backup entry '{entry.EntryPath}' failed checksum validation.");
            }

            var targetPath = GetRestorePath(entry);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            await using var sourceStream = archiveEntry.Open();
            await using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream, cancellationToken);
        }
    }

    private async Task CreateRestorePointAsync(RestoreOptions options, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.RestorePointDirectory);
        var restorePointPath = Path.Combine(_options.RestorePointDirectory, $"restore-point-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.zip");
        await CreateAsync(new BackupOptions(
            IncludeSettings: options.RestoreSettings,
            IncludeProfiles: options.RestoreProfiles,
            IncludeFavorites: options.RestoreFavorites,
            IncludeMetadata: options.RestoreMetadata,
            IncludeNotifications: options.RestoreNotifications,
            IncludeTasks: options.RestoreTasks,
            IncludeUpdates: options.RestoreUpdates,
            IncludeCache: options.RestoreCache,
            IncludeLogs: options.RestoreLogs), restorePointPath, cancellationToken);
    }

    private async Task<BackupManifest> LoadManifestAsync(ZipArchive archive, CancellationToken cancellationToken)
    {
        var entry = archive.GetEntry(ManifestPath)
            ?? throw new InvalidOperationException("Backup manifest was not found.");
        await using var stream = entry.Open();
        var manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(stream, JsonOptions, cancellationToken);

        return manifest ?? throw new InvalidOperationException("Backup manifest is empty.");
    }

    private BackupSource[] GetSources(BackupOptions options)
    {
        var sources = new List<BackupSource>();
        if (options.IncludeSettings)
        {
            sources.Add(BackupSource.File(BackupItemKind.Settings, "settings/settings.json", _settingsOptions.FilePath));
        }

        if (options.IncludeProfiles)
        {
            sources.Add(BackupSource.Directory(BackupItemKind.Profiles, "profiles", _profileOptions.RootDirectory));
        }

        if (options.IncludeFavorites)
        {
            sources.Add(BackupSource.File(BackupItemKind.Favorites, "favorites/favorites.json", _favoriteOptions.FilePath));
        }

        if (options.IncludeMetadata)
        {
            sources.Add(BackupSource.File(BackupItemKind.Metadata, "metadata/games.json", _metadataOptions.FilePath));
            sources.Add(BackupSource.File(BackupItemKind.Updates, "metadata/gametdb-state.json", _gameTdbOptions.FilePath));
        }

        if (options.IncludeNotifications)
        {
            sources.Add(BackupSource.File(BackupItemKind.Notifications, "notifications/history.json", _notificationOptions.FilePath));
        }

        if (options.IncludeTasks)
        {
            sources.Add(BackupSource.File(BackupItemKind.Tasks, "tasks/tasks.json", _taskOptions.FilePath));
        }

        if (options.IncludeUpdates)
        {
            sources.Add(BackupSource.File(BackupItemKind.Updates, "updates/app-update-state.json", _appUpdateOptions.FilePath));
        }

        if (options.IncludeCache)
        {
            sources.Add(BackupSource.Directory(BackupItemKind.Cache, "cache/assets", _coverOptions.RootDirectory));
        }

        return sources.ToArray();
    }

    private IEnumerable<string> GetFiles(BackupSource source)
    {
        if (!source.Exists)
        {
            return [];
        }

        return source.IsDirectory
            ? Directory.EnumerateFiles(source.SourcePath, "*", SearchOption.AllDirectories)
            : [source.SourcePath];
    }

    private BackupItem CreatePreviewItem(BackupSource source, string? filePath)
    {
        if (filePath is null)
        {
            return new BackupItem(source.Kind, source.EntryRoot, 0, Exists: false);
        }

        return new BackupItem(source.Kind, GetEntryPath(source, filePath), new FileInfo(filePath).Length, Exists: true);
    }

    private string GetEntryPath(BackupSource source, string filePath)
    {
        if (!source.IsDirectory)
        {
            return source.EntryRoot;
        }

        return Path.Combine(source.EntryRoot, GetFileRelativePath(source, filePath)).Replace('\\', '/');
    }

    private static string GetFileRelativePath(BackupSource source, string filePath)
    {
        return source.IsDirectory
            ? Path.GetRelativePath(source.SourcePath, filePath)
            : Path.GetFileName(source.SourcePath);
    }

    private string GetRestorePath(BackupManifestEntry entry)
    {
        return entry.Kind switch
        {
            BackupItemKind.Settings => _settingsOptions.FilePath,
            BackupItemKind.Profiles => Path.Combine(_profileOptions.RootDirectory, entry.RelativePath),
            BackupItemKind.Favorites => _favoriteOptions.FilePath,
            BackupItemKind.Metadata => entry.EntryPath.Contains("gametdb-state", StringComparison.OrdinalIgnoreCase)
                ? _gameTdbOptions.FilePath
                : _metadataOptions.FilePath,
            BackupItemKind.Notifications => _notificationOptions.FilePath,
            BackupItemKind.Tasks => _taskOptions.FilePath,
            BackupItemKind.Updates => _appUpdateOptions.FilePath,
            BackupItemKind.Cache => Path.Combine(_coverOptions.RootDirectory, entry.RelativePath),
            _ => throw new InvalidOperationException($"Unsupported backup item kind '{entry.Kind}'.")
        };
    }

    private static bool ShouldRestore(BackupItemKind kind, RestoreOptions options)
    {
        return kind switch
        {
            BackupItemKind.Settings => options.RestoreSettings,
            BackupItemKind.Profiles => options.RestoreProfiles,
            BackupItemKind.Favorites => options.RestoreFavorites,
            BackupItemKind.Metadata => options.RestoreMetadata,
            BackupItemKind.Notifications => options.RestoreNotifications,
            BackupItemKind.Tasks => options.RestoreTasks,
            BackupItemKind.Updates => options.RestoreUpdates,
            BackupItemKind.Cache => options.RestoreCache,
            BackupItemKind.Logs => options.RestoreLogs,
            _ => false
        };
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken)
    {
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static void ValidatePath(string filePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Path must be provided.", parameterName);
        }

        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException("Path contains invalid characters.", parameterName);
        }
    }

    private sealed record BackupSource(BackupItemKind Kind, string EntryRoot, string SourcePath, bool IsDirectory)
    {
        public bool Exists => IsDirectory ? System.IO.Directory.Exists(SourcePath) : System.IO.File.Exists(SourcePath);

        public static BackupSource File(BackupItemKind kind, string entryRoot, string sourcePath)
        {
            return new BackupSource(kind, entryRoot, sourcePath, IsDirectory: false);
        }

        public static BackupSource Directory(BackupItemKind kind, string entryRoot, string sourcePath)
        {
            return new BackupSource(kind, entryRoot, sourcePath, IsDirectory: true);
        }
    }

    private sealed record BackupManifest(
        string ApplicationVersion,
        DateTimeOffset CreatedAt,
        IReadOnlyList<BackupManifestEntry> Entries);

    private sealed record BackupManifestEntry(
        BackupItemKind Kind,
        string EntryPath,
        string RelativePath,
        string Sha256,
        long SizeInBytes);
}
