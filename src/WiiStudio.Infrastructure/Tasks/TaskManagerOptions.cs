namespace WiiStudio.Infrastructure.Tasks;

public sealed class TaskManagerOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "tasks",
        "tasks.json");

    public int HistoryLimit { get; init; } = 500;
}
