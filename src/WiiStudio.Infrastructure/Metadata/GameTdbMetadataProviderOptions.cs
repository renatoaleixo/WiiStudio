namespace WiiStudio.Infrastructure.Metadata;

public sealed class GameTdbMetadataProviderOptions
{
    public string DatabaseUrl { get; init; } = "https://www.gametdb.com/wiitdb.zip";

    public string PreferredLanguage { get; init; } = "PT";

    public IReadOnlyList<string> FallbackLanguages { get; init; } = ["PT", "EN", "US", "ORIG"];

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    public long MaxDatabaseZipBytes { get; init; } = 50 * 1024 * 1024;

    public bool AllowAutomaticMetadataDownload { get; init; } = true;

    public TimeSpan DatabaseTtl { get; init; } = TimeSpan.FromDays(7);

    public string DatabaseDirectory { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "metadata",
        "gametdb");
}
