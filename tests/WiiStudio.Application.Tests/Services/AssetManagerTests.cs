using FluentAssertions;
using System.Text;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class AssetManagerTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnCachedAssetWhenAvailable()
    {
        using var fixture = TemporaryAssetFixture.Create();
        var cache = new FakeCoverCache(fixture.RootDirectory);
        cache.CachedPaths[("RMCP01", CoverAssetKind.FrontCover)] = fixture.CreateFile("RMCP01.bin", "cover");
        var provider = new FakeCoverProvider();
        var manager = new AssetManager(cache, provider);

        var asset = await manager.GetAsync(
            new AssetRequest(new AssetKey("rmcp01", CoverAssetKind.FrontCover)),
            CancellationToken.None);

        asset.Should().NotBeNull();
        asset!.Key.GameId.Should().Be("RMCP01");
        asset.Source.Should().Be(CoverAssetSource.Cache);
        asset.SizeInBytes.Should().BeGreaterThan(0);
        provider.DownloadRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_ShouldDownloadAndCacheWhenMissing()
    {
        using var fixture = TemporaryAssetFixture.Create();
        var cache = new FakeCoverCache(fixture.RootDirectory);
        var provider = new FakeCoverProvider();
        provider.Assets[("RMCP01", CoverAssetKind.FrontCover)] = Encoding.UTF8.GetBytes("cover");
        var manager = new AssetManager(cache, provider);

        var asset = await manager.GetAsync(
            new AssetRequest(new AssetKey("RMCP01", CoverAssetKind.FrontCover)),
            CancellationToken.None);

        asset.Should().NotBeNull();
        asset!.Source.Should().Be(CoverAssetSource.Provider);
        cache.SavedAssets.Should().ContainSingle().Which.Should().Be(("RMCP01", CoverAssetKind.FrontCover));
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNullWhenProviderFallbackIsDisabled()
    {
        var manager = new AssetManager(new FakeCoverCache(Path.GetTempPath()), new FakeCoverProvider());

        var asset = await manager.GetAsync(
            new AssetRequest(new AssetKey("RMCP01", CoverAssetKind.FrontCover), AllowProviderFallback: false),
            CancellationToken.None);

        asset.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistAssetThroughCache()
    {
        using var fixture = TemporaryAssetFixture.Create();
        var cache = new FakeCoverCache(fixture.RootDirectory);
        var manager = new AssetManager(cache, new FakeCoverProvider());
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("cover"));

        await manager.SaveAsync(
            new AssetSaveRequest(new AssetKey("rmcp01", CoverAssetKind.FrontCover), stream),
            CancellationToken.None);

        cache.SavedAssets.Should().ContainSingle().Which.Should().Be(("RMCP01", CoverAssetKind.FrontCover));
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveAssetThroughCache()
    {
        using var fixture = TemporaryAssetFixture.Create();
        var cache = new FakeCoverCache(fixture.RootDirectory);
        cache.CachedPaths[("RMCP01", CoverAssetKind.FrontCover)] = fixture.CreateFile("RMCP01.bin", "cover");
        var manager = new AssetManager(cache, new FakeCoverProvider());

        var removed = await manager.RemoveAsync(new AssetKey("rmcp01", CoverAssetKind.FrontCover), CancellationToken.None);

        removed.Should().BeTrue();
    }

    private sealed class FakeCoverCache : ICoverCache
    {
        private readonly string _rootDirectory;

        public FakeCoverCache(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public Dictionary<(string GameId, CoverAssetKind Kind), string> CachedPaths { get; } = [];

        public List<(string GameId, CoverAssetKind Kind)> SavedAssets { get; } = [];

        public Task<string?> GetAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CachedPaths.TryGetValue((gameId, kind), out var path);

            return Task.FromResult<string?>(path);
        }

        public Task<Stream?> OpenReadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<Stream?>(null);
        }

        public async Task<string> SaveAsync(string gameId, CoverAssetKind kind, Stream content, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SavedAssets.Add((gameId, kind));
            var path = Path.Combine(_rootDirectory, $"{gameId}-{kind}.bin");
            await using var stream = File.Create(path);
            await content.CopyToAsync(stream, cancellationToken);
            CachedPaths[(gameId, kind)] = path;

            return path;
        }

        public Task<bool> RemoveAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(CachedPaths.Remove((gameId, kind)));
        }

        public Task RebuildIndexAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }

        public Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new AssetStatistics(CachedPaths.Count, 0, new Dictionary<CoverAssetKind, int>(), null));
        }
    }

    private sealed class FakeCoverProvider : ICoverProvider
    {
        public Dictionary<(string GameId, CoverAssetKind Kind), byte[]> Assets { get; } = [];

        public List<(string GameId, CoverAssetKind Kind)> DownloadRequests { get; } = [];

        public Task<Stream?> DownloadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DownloadRequests.Add((gameId, kind));

            return Task.FromResult<Stream?>(Assets.TryGetValue((gameId, kind), out var content)
                ? new MemoryStream(content)
                : null);
        }
    }

    private sealed class TemporaryAssetFixture : IDisposable
    {
        private TemporaryAssetFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            Directory.CreateDirectory(rootDirectory);
        }

        public string RootDirectory { get; }

        public static TemporaryAssetFixture Create()
        {
            return new TemporaryAssetFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-assets-{Guid.NewGuid():N}"));
        }

        public string CreateFile(string fileName, string content)
        {
            var path = Path.Combine(RootDirectory, fileName);
            File.WriteAllText(path, content);

            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }
}
