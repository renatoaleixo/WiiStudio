namespace WiiStudio.Infrastructure.GameTdb;

public sealed class GameTdbUpdateStateStoreOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "metadata",
        "gametdb-state.json");
}
