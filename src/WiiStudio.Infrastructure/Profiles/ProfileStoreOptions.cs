namespace WiiStudio.Infrastructure.Profiles;

public sealed class ProfileStoreOptions
{
    public string RootDirectory { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "profiles");
}
