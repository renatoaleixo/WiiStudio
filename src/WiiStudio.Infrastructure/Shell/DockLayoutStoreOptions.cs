namespace WiiStudio.Infrastructure.Shell;

public sealed class DockLayoutStoreOptions
{
    public string LayoutDirectoryPath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "profiles",
        "layouts");
}
