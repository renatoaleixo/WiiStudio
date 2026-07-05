namespace WiiStudio.Infrastructure.Favorites;

public sealed class FavoriteGameStoreOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "favorites.json");
}
