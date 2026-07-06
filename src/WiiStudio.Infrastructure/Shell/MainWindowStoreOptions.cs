namespace WiiStudio.Infrastructure.Shell;

public sealed class MainWindowStoreOptions
{
    public string LayoutFilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "config",
        "main-window-layout.json");
}
