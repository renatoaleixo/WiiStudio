using System.Text.RegularExpressions;
using WiiStudio.Application.DTOs;

namespace WiiStudio.Infrastructure.Covers;

public sealed partial class GameTdbCoverUrlBuilder : IGameTdbCoverUrlBuilder
{
    private readonly GameTdbCoverProviderOptions _options;

    public GameTdbCoverUrlBuilder(GameTdbCoverProviderOptions options)
    {
        _options = options;
    }

    public IReadOnlyList<Uri> BuildCandidateUrls(string gameId, CoverAssetKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameId);

        var normalizedGameId = gameId.ToUpperInvariant();
        if (!GameIdRegex().IsMatch(normalizedGameId))
        {
            throw new ArgumentException("Game ID must contain exactly six uppercase letters or numbers.", nameof(gameId));
        }

        var baseUri = CreateHttpsBaseUri(_options.BaseUrl);
        var pathTemplate = GetPathTemplate(kind);
        if (pathTemplate is null)
        {
            return [];
        }

        return BuildLanguages()
            .Select(language => BuildUri(baseUri, pathTemplate, language, normalizedGameId))
            .Distinct()
            .ToArray();
    }

    private IReadOnlyList<string> BuildLanguages()
    {
        var languages = new List<string> { NormalizeLanguage(_options.PreferredLanguage) };
        languages.AddRange(_options.FallbackLanguages.Select(NormalizeLanguage));

        return languages
            .Where(language => !string.IsNullOrWhiteSpace(language))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private string? GetPathTemplate(CoverAssetKind kind)
    {
        return kind switch
        {
            CoverAssetKind.FrontCover => "cover/{LANG}/{GAME_ID}.png",
            CoverAssetKind.BackCover => "coverfull/{LANG}/{GAME_ID}.png",
            CoverAssetKind.DiscArt => "disc/{LANG}/{GAME_ID}.png",
            CoverAssetKind.Icon => "cover/{LANG}/{GAME_ID}.png",
            CoverAssetKind.Banner => string.IsNullOrWhiteSpace(_options.BannerPathTemplate) ? null : _options.BannerPathTemplate,
            _ => null
        };
    }

    private static Uri BuildUri(Uri baseUri, string pathTemplate, string language, string gameId)
    {
        var relativePath = pathTemplate
            .Replace("{LANG}", Uri.EscapeDataString(language), StringComparison.Ordinal)
            .Replace("{GAME_ID}", Uri.EscapeDataString(gameId), StringComparison.Ordinal);

        if (relativePath.Contains("..", StringComparison.Ordinal) || relativePath.StartsWith('/'))
        {
            throw new InvalidOperationException("Cover URL template must be a safe relative path.");
        }

        return new Uri(baseUri, relativePath);
    }

    private static Uri CreateHttpsBaseUri(string value)
    {
        if (!Uri.TryCreate(value.TrimEnd('/') + "/", UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("GameTDB cover provider base URL must be an HTTPS URL.");
        }

        return uri;
    }

    private static string NormalizeLanguage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "PT-BR" => "PT",
            "EN-US" => "EN",
            "ES-ES" => "ES",
            "FR-FR" => "FR",
            "DE-DE" => "DE",
            "IT-IT" => "IT",
            "JA-JP" => "JA",
            "KO-KR" => "KO",
            "ZH-CN" => "ZH",
            var language => language
        };
    }

    [GeneratedRegex("^[A-Z0-9]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex GameIdRegex();
}
