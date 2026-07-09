using FluentAssertions;
using System.Text;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Covers;

namespace WiiStudio.Infrastructure.Tests.Covers;

public sealed class FileSystemCoverCacheTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnNullWhenAssetIsMissing()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();

        var path = await cache.GetAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        path.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistAssetAndAllowCacheHit()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("cover"));

        var savedPath = await cache.SaveAsync("RMCP01", CoverAssetKind.FrontCover, stream, CancellationToken.None);
        var cachedPath = await cache.GetAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        cachedPath.Should().Be(savedPath);
        savedPath.Should().EndWith(Path.Combine("covers", "RMCP01.png"));
        File.Exists(savedPath).Should().BeTrue();
    }

    [Fact]
    public async Task OpenReadAsync_ShouldReturnPersistedAssetStream()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("cover"));
        await cache.SaveAsync("RMCP01", CoverAssetKind.FrontCover, stream, CancellationToken.None);

        await using var cachedStream = await cache.OpenReadAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        cachedStream.Should().NotBeNull();
        using var reader = new StreamReader(cachedStream!);
        var content = await reader.ReadToEndAsync(CancellationToken.None);
        content.Should().Be("cover");
    }

    [Fact]
    public async Task OpenReadAsync_ShouldReturnNullWhenAssetIsMissing()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();

        var stream = await cache.OpenReadAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        stream.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnLegacyBinAssetAsFallback()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var directory = Path.Combine(fixture.RootDirectory, "covers");
        Directory.CreateDirectory(directory);
        var legacyPath = Path.Combine(directory, "RMCP01.bin");
        await File.WriteAllTextAsync(legacyPath, "legacy-cover", CancellationToken.None);
        var cache = fixture.CreateCache();

        var cachedPath = await cache.GetAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        cachedPath.Should().Be(legacyPath);
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeletePersistedAsset()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("cover"));
        await cache.SaveAsync("RMCP01", CoverAssetKind.FrontCover, stream, CancellationToken.None);

        var removed = await cache.RemoveAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);
        var cachedPath = await cache.GetAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        removed.Should().BeTrue();
        cachedPath.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalseWhenAssetIsMissing()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();

        var removed = await cache.RemoveAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        removed.Should().BeFalse();
    }

    [Fact]
    public async Task RebuildIndexAsync_ShouldCreateAssetDirectories()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();

        await cache.RebuildIndexAsync(CancellationToken.None);

        Directory.Exists(Path.Combine(fixture.RootDirectory, "covers")).Should().BeTrue();
        Directory.Exists(Path.Combine(fixture.RootDirectory, "covers-back")).Should().BeTrue();
        Directory.Exists(Path.Combine(fixture.RootDirectory, "discs")).Should().BeTrue();
        Directory.Exists(Path.Combine(fixture.RootDirectory, "banners")).Should().BeTrue();
        Directory.Exists(Path.Combine(fixture.RootDirectory, "thumbnails")).Should().BeTrue();
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnAssetTotalsByKind()
    {
        using var fixture = TemporaryCoverCacheFixture.Create();
        var cache = fixture.CreateCache();
        await using var frontCover = new MemoryStream(Encoding.UTF8.GetBytes("front"));
        await using var discArt = new MemoryStream(Encoding.UTF8.GetBytes("disc"));
        await cache.SaveAsync("RMCP01", CoverAssetKind.FrontCover, frontCover, CancellationToken.None);
        await cache.SaveAsync("RMCP01", CoverAssetKind.DiscArt, discArt, CancellationToken.None);

        var statistics = await cache.GetStatisticsAsync(CancellationToken.None);

        statistics.TotalAssets.Should().Be(2);
        statistics.SizeInBytes.Should().BeGreaterThan(0);
        statistics.CountByKind[CoverAssetKind.FrontCover].Should().Be(1);
        statistics.CountByKind[CoverAssetKind.DiscArt].Should().Be(1);
        statistics.LastModifiedAt.Should().NotBeNull();
    }

    private sealed class TemporaryCoverCacheFixture : IDisposable
    {
        private TemporaryCoverCacheFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public string RootDirectory { get; }

        public static TemporaryCoverCacheFixture Create()
        {
            return new TemporaryCoverCacheFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-covers-{Guid.NewGuid():N}"));
        }

        public FileSystemCoverCache CreateCache()
        {
            return new FileSystemCoverCache(new CoverCacheOptions { RootDirectory = RootDirectory });
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
