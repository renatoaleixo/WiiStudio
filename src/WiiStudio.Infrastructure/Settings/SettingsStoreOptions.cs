namespace WiiStudio.Infrastructure.Settings;

public sealed class SettingsStoreOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "config",
        "settings.json");
}
