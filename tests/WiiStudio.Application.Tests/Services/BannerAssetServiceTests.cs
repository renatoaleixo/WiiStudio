using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class BannerAssetServiceTests
{
    [Fact]
    public async Task DownloadAsync_ShouldDownloadBannerAndDiscArt()
    {
        var assetManager = new FakeAssetManager();
        assetManager.Assets[("RMCP01", CoverAssetKind.Banner)] = CreateAsset("RMCP01", CoverAssetKind.Banner, CoverAssetSource.Provider);
        assetManager.Assets[("RMCP01", CoverAssetKind.DiscArt)] = CreateAsset("RMCP01", CoverAssetKind.DiscArt, CoverAssetSource.Provider);
        var service = new BannerAssetService(assetManager);

        var result = await service.DownloadAsync(["rmcp01"], CancellationToken.None);

        result.HasFailures.Should().BeFalse();
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(item => item.Status == AssetDownloadStatus.Downloaded);
        assetManager.Requests.Should().Contain(request => request.Key.Kind == CoverAssetKind.Banner);
        assetManager.Requests.Should().Contain(request => request.Key.Kind == CoverAssetKind.DiscArt);
    }

    [Fact]
    public async Task DownloadAsync_ShouldReportMissingAssets()
    {
        var service = new BannerAssetService(new FakeAssetManager());

        var result = await service.DownloadAsync(["RMCP01"], CancellationToken.None);

        result.HasFailures.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(item => item.Status == AssetDownloadStatus.Missing);
    }

    [Fact]
    public async Task HasAssetsAsync_ShouldReturnTrueWhenBannerAndDiscArtExistInCache()
    {
        var assetManager = new FakeAssetManager();
        assetManager.Assets[("RMCP01", CoverAssetKind.Banner)] = CreateAsset("RMCP01", CoverAssetKind.Banner, CoverAssetSource.Cache);
        assetManager.Assets[("RMCP01", CoverAssetKind.DiscArt)] = CreateAsset("RMCP01", CoverAssetKind.DiscArt, CoverAssetSource.Cache);
        var service = new BannerAssetService(assetManager);

        var hasAssets = await service.HasAssetsAsync("rmcp01", CancellationToken.None);

        hasAssets.Should().BeTrue();
        assetManager.Requests.Should().OnlyContain(request => request.AllowProviderFallback == false);
    }

    [Fact]
    public async Task HasAssetsAsync_ShouldReturnFalseWhenOneAssetIsMissing()
    {
        var assetManager = new FakeAssetManager();
        assetManager.Assets[("RMCP01", CoverAssetKind.Banner)] = CreateAsset("RMCP01", CoverAssetKind.Banner, CoverAssetSource.Cache);
        var service = new BannerAssetService(assetManager);

        var hasAssets = await service.HasAssetsAsync("RMCP01", CancellationToken.None);

        hasAssets.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshAsync_ShouldForceRefreshBannerAndDiscArt()
    {
        var assetManager = new FakeAssetManager();
        assetManager.Assets[("RMCP01", CoverAssetKind.Banner)] = CreateAsset("RMCP01", CoverAssetKind.Banner, CoverAssetSource.Provider);
        assetManager.Assets[("RMCP01", CoverAssetKind.DiscArt)] = CreateAsset("RMCP01", CoverAssetKind.DiscArt, CoverAssetSource.Provider);
        var service = new BannerAssetService(assetManager);

        await service.RefreshAsync("rmcp01", CancellationToken.None);

        assetManager.Requests.Should().HaveCount(2);
        assetManager.Requests.Should().OnlyContain(request => request.ForceRefresh);
    }

    [Fact]
    public async Task DownloadAsync_ShouldRejectInvalidGameId()
    {
        var service = new BannerAssetService(new FakeAssetManager());

        var act = () => service.DownloadAsync(["invalid"], CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static AssetInfo CreateAsset(string gameId, CoverAssetKind kind, CoverAssetSource source)
    {
        return new AssetInfo(new AssetKey(gameId, kind), $"/assets/{gameId}-{kind}.bin", 1024, DateTimeOffset.UtcNow, source);
    }

    private sealed class FakeAssetManager : IAssetManager
    {
        public Dictionary<(string GameId, CoverAssetKind Kind), AssetInfo> Assets { get; } = [];

        public List<AssetRequest> Requests { get; } = [];

        public Task<AssetInfo?> GetAsync(AssetRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            Assets.TryGetValue((request.Key.GameId, request.Key.Kind), out var asset);

            return Task.FromResult<AssetInfo?>(asset);
        }

        public Task SaveAsync(AssetSaveRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<bool> RemoveAsync(AssetKey key, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task RebuildIndexAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
