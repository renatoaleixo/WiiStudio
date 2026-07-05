using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Covers;

public sealed class FileSystemCoverCache : ICoverCache
{
    private readonly CoverCacheOptions _options;

    public FileSystemCoverCache(CoverCacheOptions options)
    {
        _options = options;
    }

    public Task<string?> GetAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = GetAssetPath(gameId, kind);

        return Task.FromResult<string?>(File.Exists(path) ? path : null);
    }

    public Task<Stream?> OpenReadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = GetAssetPath(gameId, kind);
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

        var path = GetAssetPath(gameId, kind);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await content.CopyToAsync(stream, cancellationToken);

        return path;
    }

    private string GetAssetPath(string gameId, CoverAssetKind kind)
    {
        return Path.Combine(_options.RootDirectory, GetFolderName(kind), $"{gameId.ToUpperInvariant()}.bin");
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
