namespace WiiStudio.Infrastructure.Covers;

public sealed class GameTdbCoverProviderOptions
{
    public string BaseUrl { get; init; } = "https://art.gametdb.com/wii";

    public string PreferredLanguage { get; init; } = "PT";

    public IReadOnlyList<string> FallbackLanguages { get; init; } = ["PT", "EN", "US", "ORIG"];

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);

    public long MaxImageBytes { get; init; } = 5 * 1024 * 1024;

    public long MaxFullCoverBytes { get; init; } = 10 * 1024 * 1024;

    public bool AllowAutomaticCoverDownload { get; init; } = true;

    public string? BannerPathTemplate { get; init; }
}
