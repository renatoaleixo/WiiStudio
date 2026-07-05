using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class MetadataSynchronizationServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldReportCachedMetadata()
    {
        var cache = new FakeMetadataCache();
        cache.Metadata["RMCP01"] = CreateMetadata("RMCP01", "Mario Kart Wii");
        var service = new MetadataSynchronizationService(cache, []);

        var preview = await service.AnalyzeAsync(["rmcp01"], CancellationToken.None);

        var item = preview.Items.Should().ContainSingle().Subject;
        item.GameId.Should().Be("RMCP01");
        item.Status.Should().Be(MetadataSyncStatus.Cached);
        item.Source.Should().Be(MetadataSyncSource.Cache);
    }

    [Fact]
    public async Task SynchronizeAsync_ShouldReuseCacheWhenForceRefreshIsFalse()
    {
        var cache = new FakeMetadataCache();
        cache.Metadata["RMCP01"] = CreateMetadata("RMCP01", "Mario Kart Wii");
        var provider = new FakeMetadataProvider(isAvailable: true);
        var service = new MetadataSynchronizationService(cache, [provider]);

        var result = await service.SynchronizeAsync(new MetadataSyncRequest(["RMCP01"]), CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Status.Should().Be(MetadataSyncStatus.Cached);
        provider.RequestedGameIds.Should().BeEmpty();
    }

    [Fact]
    public async Task SynchronizeAsync_ShouldDownloadAndCacheWhenForceRefreshIsTrue()
    {
        var cache = new FakeMetadataCache();
        cache.Metadata["RMCP01"] = CreateMetadata("RMCP01", "Old Title");
        var provider = new FakeMetadataProvider(isAvailable: true);
        provider.Metadata["RMCP01"] = CreateMetadata("RMCP01", "Mario Kart Wii");
        var service = new MetadataSynchronizationService(cache, [provider]);

        var result = await service.SynchronizeAsync(
            new MetadataSyncRequest(["RMCP01"], ForceRefresh: true),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be(MetadataSyncStatus.Updated);
        item.Source.Should().Be(MetadataSyncSource.Provider);
        cache.Metadata["RMCP01"].Title.Should().Be("Mario Kart Wii");
    }

    [Fact]
    public async Task SynchronizeAsync_ShouldReportProviderUnavailableWhenNothingCanServeMetadata()
    {
        var service = new MetadataSynchronizationService(new FakeMetadataCache(), [new FakeMetadataProvider(isAvailable: false)]);

        var result = await service.SynchronizeAsync(new MetadataSyncRequest(["RMCP01"]), CancellationToken.None);

        result.HasFailures.Should().BeTrue();
        result.Items.Should().ContainSingle().Which.Status.Should().Be(MetadataSyncStatus.ProviderUnavailable);
    }

    [Fact]
    public async Task SynchronizeAsync_ShouldRejectInvalidGameId()
    {
        var service = new MetadataSynchronizationService(new FakeMetadataCache(), []);

        var act = () => service.SynchronizeAsync(new MetadataSyncRequest(["invalid"]), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static GameMetadataDto CreateMetadata(string gameId, string title)
    {
        return new GameMetadataDto(gameId, title, Region.Pal);
    }

    private sealed class FakeMetadataCache : IMetadataCacheService
    {
        public Dictionary<string, GameMetadataDto> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Metadata.TryGetValue(gameId, out var metadata);

            return Task.FromResult(metadata);
        }

        public Task SaveAsync(GameMetadataDto metadata, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Metadata[metadata.GameId] = metadata;

            return Task.CompletedTask;
        }

        public Task InvalidateAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Metadata.Remove(gameId);

            return Task.CompletedTask;
        }

        public Task<MetadataCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new MetadataCacheStatistics(Metadata.Count, 0, null));
        }

        public Task CleanupAsync(MetadataCleanupOptions options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }

    private sealed class FakeMetadataProvider : IGameMetadataProvider
    {
        private readonly bool _isAvailable;

        public FakeMetadataProvider(bool isAvailable)
        {
            _isAvailable = isAvailable;
        }

        public Dictionary<string, GameMetadataDto> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<string> RequestedGameIds { get; } = [];

        public Task<GameMetadataDto?> GetByGameIdAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestedGameIds.Add(gameId);
            Metadata.TryGetValue(gameId, out var metadata);

            return Task.FromResult(metadata);
        }

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_isAvailable);
        }
    }
}
