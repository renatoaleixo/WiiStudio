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
