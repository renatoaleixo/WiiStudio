namespace WiiStudio.Infrastructure.AppUpdates;

public sealed class ApplicationUpdateStateStoreOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "updates",
        "app-update-state.json");

    public string CurrentVersion { get; init; } = "0.1.0";
}
