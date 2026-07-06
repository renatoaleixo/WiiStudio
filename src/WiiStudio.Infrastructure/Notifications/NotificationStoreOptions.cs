namespace WiiStudio.Infrastructure.Notifications;

public sealed class NotificationStoreOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "notifications",
        "history.json");

    public int HistoryLimit { get; init; } = 200;
}
