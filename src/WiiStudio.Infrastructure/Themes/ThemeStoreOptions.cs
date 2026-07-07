namespace WiiStudio.Infrastructure.Themes;

public sealed class ThemeStoreOptions
{
    public string ThemeDirectoryPath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "themes");
}
