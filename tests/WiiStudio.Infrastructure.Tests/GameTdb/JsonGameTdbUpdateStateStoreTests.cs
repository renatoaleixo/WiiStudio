using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.GameTdb;

namespace WiiStudio.Infrastructure.Tests.GameTdb;

public sealed class JsonGameTdbUpdateStateStoreTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnEmptyStateWhenFileDoesNotExist()
    {
        var path = CreateTemporaryPath();
        var store = new JsonGameTdbUpdateStateStore(new GameTdbUpdateStateStoreOptions { FilePath = path });

        var state = await store.LoadAsync(CancellationToken.None);

        state.InstalledVersion.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistState()
    {
        var path = CreateTemporaryPath();
        try
        {
            var store = new JsonGameTdbUpdateStateStore(new GameTdbUpdateStateStoreOptions { FilePath = path });

            await store.SaveAsync(new GameTdbUpdateState(InstalledVersion: "1.1.0", PreviousVersion: "1.0.0"), CancellationToken.None);
            var state = await store.LoadAsync(CancellationToken.None);

            state.InstalledVersion.Should().Be("1.1.0");
            state.PreviousVersion.Should().Be("1.0.0");
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public async Task RollbackAsync_ShouldSwapInstalledAndPreviousVersions()
    {
        var path = CreateTemporaryPath();
        try
        {
            var store = new JsonGameTdbUpdateStateStore(new GameTdbUpdateStateStoreOptions { FilePath = path });
            await store.SaveAsync(new GameTdbUpdateState(InstalledVersion: "1.1.0", PreviousVersion: "1.0.0"), CancellationToken.None);

            await store.RollbackAsync(CancellationToken.None);

            var state = await store.LoadAsync(CancellationToken.None);
            state.InstalledVersion.Should().Be("1.0.0");
            state.PreviousVersion.Should().Be("1.1.0");
            state.LastUpdatedAt.Should().NotBeNull();
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    private static string CreateTemporaryPath()
    {
        return Path.Combine(Path.GetTempPath(), $"wiistudio-gametdb-{Guid.NewGuid():N}", "state.json");
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
