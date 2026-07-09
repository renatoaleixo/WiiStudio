namespace WiiStudio.Infrastructure.Metadata;

public sealed record GameTdbMetadataMappingContext(
    string PreferredLanguage,
    IReadOnlyList<string> FallbackLanguages,
    string GameTdbVersion,
    DateTimeOffset UpdatedAt);
