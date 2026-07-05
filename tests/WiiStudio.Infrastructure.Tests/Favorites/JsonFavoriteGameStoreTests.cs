using FluentAssertions;
using WiiStudio.Infrastructure.Favorites;

namespace WiiStudio.Infrastructure.Tests.Favorites;

public sealed class JsonFavoriteGameStoreTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnEmptySetWhenFileDoesNotExist()
    {
        var path = CreateTemporaryPath();
        var store = new JsonFavoriteGameStore(new FavoriteGameStoreOptions { FilePath = path });

        var gameIds = await store.LoadAsync(CancellationToken.None);

        gameIds.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistOrderedGameIds()
    {
        var path = CreateTemporaryPath();
        try
        {
            var store = new JsonFavoriteGameStore(new FavoriteGameStoreOptions { FilePath = path });

            await store.SaveAsync(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "RZDP01", "RMCP01" }, CancellationToken.None);
            var gameIds = await store.LoadAsync(CancellationToken.None);

            gameIds.Should().BeEquivalentTo(["RMCP01", "RZDP01"]);
            var json = await File.ReadAllTextAsync(path);
            json.Should().Contain("RMCP01");
            json.Should().Contain("RZDP01");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static string CreateTemporaryPath()
    {
        return Path.Combine(Path.GetTempPath(), $"wiistudio-favorites-{Guid.NewGuid():N}", "favorites.json");
    }
}
