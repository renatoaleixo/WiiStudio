using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class GameTdbUpdateServiceTests
{
    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReportAvailableWhenLatestVersionIsNewer()
    {
        var stateStore = new FakeStateStore(new GameTdbUpdateState(InstalledVersion: "1.0.0"));
        var source = new FakeUpdateSource { Latest = CreateInfo("1.1.0") };
        var service = CreateService(source, stateStore);

        var result = await service.CheckForUpdatesAsync(CancellationToken.None);

        result.IsAvailable.Should().BeTrue();
        result.InstalledVersion.Should().Be("1.0.0");
        result.Latest!.Version.Should().Be("1.1.0");
        stateStore.State.LastCheckedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpToDateWhenLatestIsAlreadyInstalled()
    {
        var stateStore = new FakeStateStore(new GameTdbUpdateState(InstalledVersion: "1.1.0"));
        var source = new FakeUpdateSource { Latest = CreateInfo("1.1.0") };
        var service = CreateService(source, stateStore);

        var result = await service.UpdateAsync(new GameTdbUpdateOptions(), CancellationToken.None);

        result.Status.Should().Be(GameTdbUpdateStatus.UpToDate);
        source.DownloadRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnMirrorUnavailableWhenNoLatestVersionExists()
    {
        var service = CreateService(new FakeUpdateSource(), new FakeStateStore(new GameTdbUpdateState()));

        var result = await service.UpdateAsync(new GameTdbUpdateOptions(), CancellationToken.None);

        result.Status.Should().Be(GameTdbUpdateStatus.MirrorUnavailable);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectInvalidChecksum()
    {
        var latest = CreateInfo("1.1.0", checksum: "expected");
        var source = new FakeUpdateSource
        {
            Latest = latest,
            Package = new GameTdbUpdatePackage(latest, [CreateMetadata("RMCP01")], Checksum: "actual")
        };
        var service = CreateService(source, new FakeStateStore(new GameTdbUpdateState(InstalledVersion: "1.0.0")));

        var result = await service.UpdateAsync(new GameTdbUpdateOptions(), CancellationToken.None);

        result.Status.Should().Be(GameTdbUpdateStatus.InvalidChecksum);
        result.MetadataUpdated.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldApplyPackageToMetadataCacheAndRebuildAssetIndex()
    {
        var latest = CreateInfo("1.1.0", checksum: "checksum");
        var source = new FakeUpdateSource
        {
            Latest = latest,
            Package = new GameTdbUpdatePackage(latest, [CreateMetadata("RMCP01")], Checksum: "checksum")
        };
        var stateStore = new FakeStateStore(new GameTdbUpdateState(InstalledVersion: "1.0.0"));
        var metadataCache = new FakeMetadataCache();
        var assetManager = new FakeAssetManager();
        var service = CreateService(source, stateStore, metadataCache, assetManager);

        var result = await service.UpdateAsync(new GameTdbUpdateOptions(), CancellationToken.None);

        result.Status.Should().Be(GameTdbUpdateStatus.Applied);
        result.MetadataUpdated.Should().Be(1);
        metadataCache.Metadata.Should().ContainSingle().Which.GameTdbVersion.Should().Be("1.1.0");
        assetManager.RebuildCount.Should().Be(1);
        stateStore.State.InstalledVersion.Should().Be("1.1.0");
        stateStore.State.PreviousVersion.Should().Be("1.0.0");
    }

    [Fact]
    public async Task RollbackAsync_ShouldDelegateToStateStore()
    {
        var stateStore = new FakeStateStore(new GameTdbUpdateState(InstalledVersion: "1.1.0", PreviousVersion: "1.0.0"));
        var service = CreateService(new FakeUpdateSource(), stateStore);

        await service.RollbackAsync(CancellationToken.None);

        stateStore.RollbackCount.Should().Be(1);
    }

    private static GameTdbUpdateService CreateService(
        IGameTdbUpdateSource source,
        IGameTdbUpdateStateStore stateStore,
        IMetadataCacheService? metadataCache = null,
        IAssetManager? assetManager = null)
    {
        return new GameTdbUpdateService(
            source,
            stateStore,
            metadataCache ?? new FakeMetadataCache(),
            assetManager ?? new FakeAssetManager());
    }

    private static GameTdbUpdateInfo CreateInfo(string version, string? checksum = null)
    {
        return new GameTdbUpdateInfo(version, DateTimeOffset.UtcNow, "mirror", checksum);
    }

    private static GameMetadataDto CreateMetadata(string gameId)
    {
        return new GameMetadataDto(gameId, "Mario Kart Wii", Region.Pal);
    }

    private sealed class FakeUpdateSource : IGameTdbUpdateSource
    {
        public GameTdbUpdateInfo? Latest { get; init; }

        public GameTdbUpdatePackage? Package { get; init; }

        public List<GameTdbUpdateInfo> DownloadRequests { get; } = [];

        public Task<GameTdbUpdateInfo?> GetLatestAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Latest);
        }

        public Task<GameTdbUpdatePackage?> DownloadAsync(GameTdbUpdateInfo update, GameTdbUpdateOptions options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DownloadRequests.Add(update);

            return Task.FromResult(Package);
        }
    }

    private sealed class FakeStateStore : IGameTdbUpdateStateStore
    {
        public FakeStateStore(GameTdbUpdateState state)
        {
            State = state;
        }

        public GameTdbUpdateState State { get; private set; }

        public int RollbackCount { get; private set; }

        public Task<GameTdbUpdateState> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(State);
        }

        public Task SaveAsync(GameTdbUpdateState state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            State = state;

            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RollbackCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeMetadataCache : IMetadataCacheService
    {
        public List<GameMetadataDto> Metadata { get; } = [];

        public Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<GameMetadataDto?>(Metadata.FirstOrDefault(metadata => metadata.GameId == gameId));
        }

        public Task SaveAsync(GameMetadataDto metadata, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Metadata.Add(metadata);

            return Task.CompletedTask;
        }

        public Task InvalidateAsync(string gameId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<MetadataCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task CleanupAsync(MetadataCleanupOptions options, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeAssetManager : IAssetManager
    {
        public int RebuildCount { get; private set; }

        public Task<AssetInfo?> GetAsync(AssetRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
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
            cancellationToken.ThrowIfCancellationRequested();
            RebuildCount++;

            return Task.CompletedTask;
        }

        public Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
