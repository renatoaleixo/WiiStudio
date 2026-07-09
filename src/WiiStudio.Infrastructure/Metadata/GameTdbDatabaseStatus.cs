namespace WiiStudio.Infrastructure.Metadata;

public sealed record GameTdbDatabaseStatus(
    bool IsAvailable,
    string? XmlPath,
    DateTimeOffset? DownloadedAt,
    string? SourceVersion,
    string? Message);
