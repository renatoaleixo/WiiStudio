namespace WiiStudio.Infrastructure.Commands;

public sealed class CommandHistoryStoreOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "commands",
        "recent-commands.json");
}
