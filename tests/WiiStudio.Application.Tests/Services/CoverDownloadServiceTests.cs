using FluentAssertions;
using System.Text;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class CoverDownloadServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldReportCachedAssets()
    {
        var cache = new FakeCoverCache();
        cache.CachedPaths[("RMCP01", CoverAssetKind.FrontCover)] = "/cache/RMCP01.bin";
        var service = new CoverDownloadService(cache, new FakeCoverProvider());

        var preview = await service.AnalyzeAsync(["RMCP01"], CancellationToken.None);

        preview.Items.Should().Contain(item =>
            item.GameId == "RMCP01" &&
            item.Kind == CoverAssetKind.FrontCover &&
            item.Source == CoverAssetSource.Cache &&
            item.LocalPath == "/cache/RMCP01.bin");
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldReportProviderForMissingAssets()
    {
        var service = new CoverDownloadService(new FakeCoverCache(), new FakeCoverProvider());

        var preview = await service.AnalyzeAsync(["RMCP01"], CancellationToken.None);

        preview.Items.Should().Contain(item => item.Kind == CoverAssetKind.FrontCover && item.Source == CoverAssetSource.Provider);
    }

    [Fact]
    public async Task DownloadAsync_ShouldReuseCacheWhenForceRefreshIsFalse()
    {
        var cache = new FakeCoverCache();
        cache.CachedPaths[("RMCP01", CoverAssetKind.FrontCover)] = "/cache/RMCP01.bin";
        var provider = new FakeCoverProvider();
        var service = new CoverDownloadService(cache, provider);

        var result = await service.DownloadAsync(
            new CoverDownloadRequest(["RMCP01"], ForceRefresh: false, [CoverAssetKind.FrontCover]),
            CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Source.Should().Be(CoverAssetSource.Cache);
        provider.DownloadRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task DownloadAsync_ShouldDownloadAndSaveWhenCacheMisses()
    {
        var cache = new FakeCoverCache();
        var provider = new FakeCoverProvider();
        provider.Assets[("RMCP01", CoverAssetKind.FrontCover)] = new MemoryStream(Encoding.UTF8.GetBytes("cover"));
        var service = new CoverDownloadService(cache, provider);

        var result = await service.DownloadAsync(
            new CoverDownloadRequest(["RMCP01"], ForceRefresh: false, [CoverAssetKind.FrontCover]),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle().Subject;
        item.Source.Should().Be(CoverAssetSource.Provider);
        item.LocalPath.Should().Be("/cache/RMCP01-FrontCover.bin");
        cache.SavedAssets.Should().ContainSingle().Which.Should().Be(("RMCP01", CoverAssetKind.FrontCover));
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnMissingWhenProviderDoesNotHaveAsset()
    {
        var service = new CoverDownloadService(new FakeCoverCache(), new FakeCoverProvider());

        var result = await service.DownloadAsync(
            new CoverDownloadRequest(["RMCP01"], ForceRefresh: true, [CoverAssetKind.FrontCover]),
            CancellationToken.None);

        result.HasFailures.Should().BeTrue();
        result.Items.Should().ContainSingle().Which.Source.Should().Be(CoverAssetSource.Missing);
    }

    [Fact]
    public async Task DownloadAsync_ShouldRejectInvalidGameId()
    {
        var service = new CoverDownloadService(new FakeCoverCache(), new FakeCoverProvider());

        var act = () => service.DownloadAsync(new CoverDownloadRequest(["invalid"]), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class FakeCoverCache : ICoverCache
    {
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

        public Task<string> SaveAsync(string gameId, CoverAssetKind kind, Stream content, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SavedAssets.Add((gameId, kind));
            var path = $"/cache/{gameId}-{kind}.bin";
            CachedPaths[(gameId, kind)] = path;

            return Task.FromResult(path);
        }
    }

    private sealed class FakeCoverProvider : ICoverProvider
    {
        public Dictionary<(string GameId, CoverAssetKind Kind), Stream> Assets { get; } = [];

        public List<(string GameId, CoverAssetKind Kind)> DownloadRequests { get; } = [];

        public Task<Stream?> DownloadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DownloadRequests.Add((gameId, kind));
            Assets.TryGetValue((gameId, kind), out var stream);

            return Task.FromResult<Stream?>(stream);
        }
    }
}
