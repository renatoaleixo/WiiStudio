using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Infrastructure.Metadata;

public sealed class GameTdbMetadataMapper : IGameTdbMetadataMapper
{
    public GameMetadataDto Map(GameTdbGameXmlEntry source, GameTdbMetadataMappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        var locale = SelectLocale(source, context);
        var title = FirstNonEmpty(locale?.Title, source.InternalName, source.GameId) ?? source.GameId;
        var description = locale?.Synopsis;

        return new GameMetadataDto(
            source.GameId.ToUpperInvariant(),
            title,
            MapRegion(source.Region, source.GameId),
            Developer: source.Developer,
            Publisher: source.Publisher,
            Year: ParseYear(source.Year ?? source.Date),
            Languages: SplitValues(source.Languages),
            Genres: SplitValues(source.Genre),
            Players: ParsePlayers(source.Players),
            Rating: source.Rating,
            Description: description,
            OnlineFeatures: source.OnlineFeatures,
            Compatibility: null,
            GameTdbVersion: context.GameTdbVersion,
            UpdatedAt: context.UpdatedAt);
    }

    private static GameTdbLocaleXmlEntry? SelectLocale(
        GameTdbGameXmlEntry source,
        GameTdbMetadataMappingContext context)
    {
        foreach (var language in BuildLanguageFallback(context))
        {
            if (source.Locales.TryGetValue(language, out var locale) &&
                (!string.IsNullOrWhiteSpace(locale.Title) || !string.IsNullOrWhiteSpace(locale.Synopsis)))
            {
                return locale;
            }
        }

        return source.Locales.Values.FirstOrDefault();
    }

    private static IReadOnlyList<string> BuildLanguageFallback(GameTdbMetadataMappingContext context)
    {
        var languages = new List<string> { NormalizeLanguage(context.PreferredLanguage) };
        languages.AddRange(context.FallbackLanguages.Select(NormalizeLanguage));

        return languages
            .Where(language => !string.IsNullOrWhiteSpace(language))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static Region MapRegion(string? value, string gameId)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (normalized is "PAL" or "EU" or "EUR" or "EUROPE")
        {
            return Region.Pal;
        }

        if (normalized is "NTSC-U" or "USA" or "US")
        {
            return Region.NtscU;
        }

        if (normalized is "NTSC-J" or "JAPAN" or "JP")
        {
            return Region.NtscJ;
        }

        if (normalized is "KOR" or "KOREA")
        {
            return Region.Kor;
        }

        return gameId.Length > 3
            ? gameId[3] switch
            {
                'P' => Region.Pal,
                'E' => Region.NtscU,
                'J' => Region.NtscJ,
                'K' => Region.Kor,
                _ => Region.Unknown
            }
            : Region.Unknown;
    }

    private static int? ParseYear(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = System.Text.RegularExpressions.Regex.Match(value, @"\d{4}");
        return match.Success && int.TryParse(match.Value, out var year) ? year : null;
    }

    private static int? ParsePlayers(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = System.Text.RegularExpressions.Regex.Match(value, @"\d+");
        return match.Success && int.TryParse(match.Value, out var players) ? players : null;
    }

    private static IReadOnlyList<string>? SplitValues(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var values = value
            .Split([',', ';', '/', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return values.Length == 0 ? null : values;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string NormalizeLanguage(string value)
    {
        return value.Trim().ToUpperInvariant() switch
        {
            "PT-BR" => "PT",
            "EN-US" => "EN",
            var language => language
        };
    }
}
