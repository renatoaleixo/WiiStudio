using System.Globalization;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Infrastructure.Wiimms;

internal static class WitGameListParser
{
    public static IReadOnlyList<Game> Parse(string standardOutput)
    {
        ArgumentNullException.ThrowIfNull(standardOutput);

        var games = new List<Game>();

        foreach (var line in standardOutput.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (IsHeaderOrComment(line))
            {
                continue;
            }

            var columns = line.Split('\t', StringSplitOptions.TrimEntries);
            if (columns.Length < 6)
            {
                continue;
            }

            if (!TryParse(columns, out var game))
            {
                continue;
            }

            games.Add(game);
        }

        return games;
    }

    private static bool TryParse(string[] columns, out Game game)
    {
        game = default!;

        if (!long.TryParse(columns[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var sizeInBytes))
        {
            return false;
        }

        try
        {
            game = new Game(
                new GameId(columns[0]),
                columns[1],
                ParseRegion(columns[2]),
                DiscType.Wii,
                ParseFormat(columns[3]),
                columns[5],
                sizeInBytes,
                DateTimeOffset.UtcNow);

            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsHeaderOrComment(string line)
    {
        return line.StartsWith('#') || line.StartsWith("ID\t", StringComparison.OrdinalIgnoreCase);
    }

    private static Region ParseRegion(string value)
    {
        return value.Trim().ToUpperInvariant() switch
        {
            "NTSC-U" or "USA" or "U" => Region.NtscU,
            "PAL" or "EUR" or "P" => Region.Pal,
            "NTSC-J" or "JPN" or "J" => Region.NtscJ,
            "KOR" or "K" => Region.Kor,
            _ => Region.Unknown
        };
    }

    private static FileFormat ParseFormat(string value)
    {
        return value.Trim().ToUpperInvariant() switch
        {
            "ISO" => FileFormat.Iso,
            "WBFS" => FileFormat.Wbfs,
            "RVZ" => FileFormat.Rvz,
            "NKIT" or "N-KIT" => FileFormat.NKit,
            _ => FileFormat.Unknown
        };
    }
}
