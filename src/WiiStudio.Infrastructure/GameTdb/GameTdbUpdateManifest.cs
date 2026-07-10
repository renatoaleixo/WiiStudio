namespace WiiStudio.Infrastructure.GameTdb;

public sealed record GameTdbUpdateManifest(
    string Provider,
    string SourceUrl,
    DateTimeOffset DownloadedAt,
    string Version,
    int GameCount,
    string XmlSha256,
    string ZipSha256,
    long FileSizeBytes);
