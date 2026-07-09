namespace WiiStudio.Infrastructure.Metadata;

public sealed record GameTdbDatabaseManifest(
    string Provider,
    string DatabaseUrl,
    DateTimeOffset DownloadedAt,
    string PreferredLanguage,
    int GameCount,
    string SourceVersion);
