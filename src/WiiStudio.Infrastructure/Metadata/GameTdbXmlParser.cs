using System.Xml;
using System.Xml.Linq;
using WiiStudio.Application.Validators;

namespace WiiStudio.Infrastructure.Metadata;

public sealed class GameTdbXmlParser : IGameTdbXmlParser
{
    public Task<GameTdbGameXmlEntry?> FindByGameIdAsync(Stream xmlStream, string gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedGameId = GameIdValidation.Normalize(gameId);

        var settings = new XmlReaderSettings
        {
            Async = false,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(xmlStream, settings);
        var document = XDocument.Load(reader, LoadOptions.None);
        var game = document
            .Descendants()
            .FirstOrDefault(element =>
                IsNamed(element, "game") &&
                string.Equals(ReadGameId(element), normalizedGameId, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(game is null ? null : MapGame(game, normalizedGameId));
    }

    private static GameTdbGameXmlEntry MapGame(XElement game, string normalizedGameId)
    {
        var locales = game
            .Elements()
            .Where(element => IsNamed(element, "locale"))
            .Select(MapLocale)
            .Where(locale => !string.IsNullOrWhiteSpace(locale.Language))
            .GroupBy(locale => locale.Language, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

        return new GameTdbGameXmlEntry(
            normalizedGameId,
            ReadAttribute(game, "name") ?? ReadChildValue(game, "name"),
            ReadChildValue(game, "region") ?? ReadAttribute(game, "region"),
            ReadChildValue(game, "developer"),
            ReadChildValue(game, "publisher"),
            ReadDate(game),
            ReadYear(game),
            ReadChildValue(game, "languages"),
            ReadChildValue(game, "genre"),
            ReadPlayers(game),
            ReadRating(game),
            ReadOnlineFeatures(game),
            locales);
    }

    private static GameTdbLocaleXmlEntry MapLocale(XElement locale)
    {
        var language = NormalizeLanguage(ReadAttribute(locale, "lang") ?? ReadAttribute(locale, "locale") ?? string.Empty);
        return new GameTdbLocaleXmlEntry(
            language,
            ReadChildValue(locale, "title"),
            ReadChildValue(locale, "synopsis"));
    }

    private static string? ReadGameId(XElement game)
    {
        return ReadChildValue(game, "id") ?? ReadAttribute(game, "id");
    }

    private static string? ReadDate(XElement game)
    {
        var date = game.Elements().FirstOrDefault(element => IsNamed(element, "date"));
        if (date is null)
        {
            return null;
        }

        return Clean(date.Value) ??
            Clean(string.Join("-", new[] { ReadAttribute(date, "year"), ReadAttribute(date, "month"), ReadAttribute(date, "day") }
                .Where(value => !string.IsNullOrWhiteSpace(value))));
    }

    private static string? ReadYear(XElement game)
    {
        var date = game.Elements().FirstOrDefault(element => IsNamed(element, "date"));
        return date is null ? null : ReadAttribute(date, "year");
    }

    private static string? ReadPlayers(XElement game)
    {
        var input = game.Descendants().FirstOrDefault(element => IsNamed(element, "input"));
        return input is null
            ? ReadChildValue(game, "players")
            : ReadAttribute(input, "players") ?? ReadChildValue(input, "players");
    }

    private static string? ReadRating(XElement game)
    {
        var rating = game.Descendants().FirstOrDefault(element => IsNamed(element, "rating"));
        if (rating is null)
        {
            return null;
        }

        var type = ReadAttribute(rating, "type");
        var value = ReadAttribute(rating, "value") ?? Clean(rating.Value);

        return string.IsNullOrWhiteSpace(type) ? value : $"{type}: {value}";
    }

    private static string? ReadOnlineFeatures(XElement game)
    {
        var wifi = game.Descendants().FirstOrDefault(element => IsNamed(element, "wi-fi") || IsNamed(element, "wifi"));
        if (wifi is null)
        {
            return ReadChildValue(game, "online");
        }

        var players = ReadAttribute(wifi, "players");
        var features = wifi.Elements()
            .Where(element => IsNamed(element, "feature"))
            .Select(element => Clean(element.Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(players))
        {
            parts.Add($"Online players: {players}");
        }

        parts.AddRange(features!);

        return parts.Count == 0 ? null : string.Join("; ", parts);
    }

    private static string? ReadChildValue(XElement parent, string localName)
    {
        return Clean(parent.Elements().FirstOrDefault(element => IsNamed(element, localName))?.Value);
    }

    private static string? ReadAttribute(XElement element, string localName)
    {
        return Clean(element.Attributes().FirstOrDefault(attribute =>
            string.Equals(attribute.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))?.Value);
    }

    private static bool IsNamed(XElement element, string localName)
    {
        return string.Equals(element.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase);
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

    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}
