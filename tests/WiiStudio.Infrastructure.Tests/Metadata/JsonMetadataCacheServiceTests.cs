using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Metadata;

namespace WiiStudio.Infrastructure.Tests.Metadata;

public sealed class JsonMetadataCacheServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnNullWhenCacheDoesNotExist()
    {
        var path = CreateTemporaryPath();
        var cache = new JsonMetadataCacheService(new MetadataCacheOptions { FilePath = path });

        var metadata = await cache.GetAsync("RMCP01", CancellationToken.None);

        metadata.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistMetadataAndNormalizeGameId()
    {
        var path = CreateTemporaryPath();
        try
        {
            var cache = new JsonMetadataCacheService(new MetadataCacheOptions { FilePath = path });

            await cache.SaveAsync(new GameMetadataDto("rmcp01", "Mario Kart Wii", Region.Pal), CancellationToken.None);
            var metadata = await cache.GetAsync("RMCP01", CancellationToken.None);

            metadata.Should().NotBeNull();
            metadata!.GameId.Should().Be("RMCP01");
            metadata.Title.Should().Be("Mario Kart Wii");
            metadata.UpdatedAt.Should().NotBeNull();
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public async Task InvalidateAsync_ShouldRemoveMetadata()
    {
        var path = CreateTemporaryPath();
        try
        {
            var cache = new JsonMetadataCacheService(new MetadataCacheOptions { FilePath = path });
            await cache.SaveAsync(new GameMetadataDto("RMCP01", "Mario Kart Wii", Region.Pal), CancellationToken.None);

            await cache.InvalidateAsync("RMCP01", CancellationToken.None);

            var metadata = await cache.GetAsync("RMCP01", CancellationToken.None);
            metadata.Should().BeNull();
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCacheTotals()
    {
        var path = CreateTemporaryPath();
        try
        {
            var cache = new JsonMetadataCacheService(new MetadataCacheOptions { FilePath = path });
            await cache.SaveAsync(new GameMetadataDto("RMCP01", "Mario Kart Wii", Region.Pal), CancellationToken.None);

            var statistics = await cache.GetStatisticsAsync(CancellationToken.None);

            statistics.TotalGames.Should().Be(1);
            statistics.SizeInBytes.Should().BeGreaterThan(0);
            statistics.LastUpdatedAt.Should().NotBeNull();
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public async Task CleanupAsync_ShouldRemoveAllEntriesWhenRemoveExpiredOnlyIsFalse()
    {
        var path = CreateTemporaryPath();
        try
        {
            var cache = new JsonMetadataCacheService(new MetadataCacheOptions { FilePath = path });
            await cache.SaveAsync(new GameMetadataDto("RMCP01", "Mario Kart Wii", Region.Pal), CancellationToken.None);

            await cache.CleanupAsync(new MetadataCleanupOptions(RemoveExpiredOnly: false), CancellationToken.None);

            var statistics = await cache.GetStatisticsAsync(CancellationToken.None);
            statistics.TotalGames.Should().Be(0);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    private static string CreateTemporaryPath()
    {
        return Path.Combine(Path.GetTempPath(), $"wiistudio-metadata-{Guid.NewGuid():N}", "games.json");
    }

    private static void DeleteTemporaryDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
