using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Covers;

public sealed class FileSystemCoverCache : ICoverCache
{
    private static readonly string[] SupportedExtensions = [".png", ".jpg", ".jpeg", ".webp", ".bin"];
    private readonly CoverCacheOptions _options;

    public FileSystemCoverCache(CoverCacheOptions options)
    {
        _options = options;
    }

    public Task<string?> GetAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = FindAssetPath(gameId, kind);

        return Task.FromResult<string?>(File.Exists(path) ? path : null);
    }

    public Task<Stream?> OpenReadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = FindAssetPath(gameId, kind);
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(path);

        return Task.FromResult<Stream?>(stream);
    }

    public async Task<string> SaveAsync(string gameId, CoverAssetKind kind, Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        cancellationToken.ThrowIfCancellationRequested();

        var path = GetAssetPath(gameId, kind, ".png");
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await content.CopyToAsync(stream, cancellationToken);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        return path;
    }

    public Task<bool> RemoveAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = FindAssetPath(gameId, kind);
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);

        return Task.FromResult(true);
    }

    public Task RebuildIndexAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var kind in Enum.GetValues<CoverAssetKind>())
        {
            Directory.CreateDirectory(Path.Combine(_options.RootDirectory, GetFolderName(kind)));
        }

        return Task.CompletedTask;
    }

    public Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var countByKind = Enum.GetValues<CoverAssetKind>()
            .ToDictionary(kind => kind, _ => 0);
        var totalAssets = 0;
        var sizeInBytes = 0L;
        DateTimeOffset? lastModifiedAt = null;

        foreach (var kind in Enum.GetValues<CoverAssetKind>())
        {
            var directory = Path.Combine(_options.RootDirectory, GetFolderName(kind));
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(path => SupportedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileInfo = new FileInfo(path);
                totalAssets++;
                countByKind[kind]++;
                sizeInBytes += fileInfo.Length;

                var modifiedAt = new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero);
                if (lastModifiedAt is null || modifiedAt > lastModifiedAt)
                {
                    lastModifiedAt = modifiedAt;
                }
            }
        }

        return Task.FromResult(new AssetStatistics(totalAssets, sizeInBytes, countByKind, lastModifiedAt));
    }

    private string FindAssetPath(string gameId, CoverAssetKind kind)
    {
        foreach (var extension in SupportedExtensions)
        {
            var path = GetAssetPath(gameId, kind, extension);
            if (File.Exists(path))
            {
                return path;
            }
        }

        return GetAssetPath(gameId, kind, ".png");
    }

    private string GetAssetPath(string gameId, CoverAssetKind kind, string extension)
    {
        var safeGameId = gameId.ToUpperInvariant();
        if (safeGameId.Length != 6 || safeGameId.Any(character => !char.IsAsciiLetterOrDigit(character)))
        {
            throw new ArgumentException("Game ID must contain exactly six letters or numbers.", nameof(gameId));
        }

        return Path.Combine(_options.RootDirectory, GetFolderName(kind), $"{safeGameId}{extension}");
    }

    private static string GetFolderName(CoverAssetKind kind)
    {
        return kind switch
        {
            CoverAssetKind.FrontCover => "covers",
            CoverAssetKind.BackCover => "covers-back",
            CoverAssetKind.DiscArt => "discs",
            CoverAssetKind.Banner => "banners",
            CoverAssetKind.Icon => "thumbnails",
            _ => "other"
        };
    }
}
