namespace WiiStudio.Infrastructure.Metadata;

public sealed class MetadataCacheOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "metadata",
        "games.json");

    public TimeSpan Expiration { get; init; } = TimeSpan.FromDays(30);
}
