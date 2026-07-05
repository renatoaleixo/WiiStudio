using FluentAssertions;
using System.Text;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class CoverServiceTests
{
    [Fact]
    public async Task GetCoverAsync_ShouldReturnFrontCoverFromCacheByDefault()
    {
        var cache = new FakeCoverCache();
        cache.Assets[("RMCP01", CoverAssetKind.FrontCover)] = Encoding.UTF8.GetBytes("front");
        var service = new CoverService(cache);

        await using var stream = await service.GetCoverAsync("rmcp01", CancellationToken.None);

        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync(CancellationToken.None);
        content.Should().Be("front");
    }

    [Fact]
    public async Task GetCoverAsync_ShouldReturnSelectedAssetKindFromCache()
    {
        var cache = new FakeCoverCache();
        cache.Assets[("RMCP01", CoverAssetKind.DiscArt)] = Encoding.UTF8.GetBytes("disc");
        var service = new CoverService(cache);

        await using var stream = await service.GetCoverAsync("RMCP01", CoverAssetKind.DiscArt, CancellationToken.None);

        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync(CancellationToken.None);
        content.Should().Be("disc");
    }

    [Fact]
    public async Task GetCoverAsync_ShouldReturnNullWhenAssetIsMissing()
    {
        var service = new CoverService(new FakeCoverCache());

        var stream = await service.GetCoverAsync("RMCP01", CancellationToken.None);

        stream.Should().BeNull();
    }

    [Fact]
    public async Task GetCoverAsync_ShouldRejectInvalidGameId()
    {
        var service = new CoverService(new FakeCoverCache());

        var act = () => service.GetCoverAsync("invalid", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class FakeCoverCache : ICoverCache
    {
        public Dictionary<(string GameId, CoverAssetKind Kind), byte[]> Assets { get; } = [];

        public Task<string?> GetAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<string?>(Assets.ContainsKey((gameId, kind)) ? $"/cache/{gameId}-{kind}.bin" : null);
        }

        public Task<Stream?> OpenReadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<Stream?>(Assets.TryGetValue((gameId, kind), out var content)
                ? new MemoryStream(content)
                : null);
        }

        public Task<string> SaveAsync(string gameId, CoverAssetKind kind, Stream content, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<bool> RemoveAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Assets.Remove((gameId, kind)));
        }

        public Task RebuildIndexAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }

        public Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new AssetStatistics(Assets.Count, 0, new Dictionary<CoverAssetKind, int>(), null));
        }
    }
}
