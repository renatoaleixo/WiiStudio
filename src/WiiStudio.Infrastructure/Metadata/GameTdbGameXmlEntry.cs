namespace WiiStudio.Infrastructure.Metadata;

public sealed record GameTdbGameXmlEntry(
    string GameId,
    string? InternalName,
    string? Region,
    string? Developer,
    string? Publisher,
    string? Date,
    string? Year,
    string? Languages,
    string? Genre,
    string? Players,
    string? Rating,
    string? OnlineFeatures,
    IReadOnlyDictionary<string, GameTdbLocaleXmlEntry> Locales);

public sealed record GameTdbLocaleXmlEntry(
    string Language,
    string? Title,
    string? Synopsis);
