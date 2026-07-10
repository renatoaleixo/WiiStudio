using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Metadata;

namespace WiiStudio.Infrastructure.GameTdb;

public sealed class GameTdbUpdateSource : IGameTdbUpdateSource
{
    private const string ProviderName = "GameTDB";
    private const string ZipFileName = "wiitdb.zip";
    private const string XmlFileName = "wiitdb.xml";
    private const string ManifestFileName = "manifest.json";
    private const int MinimumGameCount = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly HttpClient _httpClient;
    private readonly GameTdbMetadataProviderOptions _options;
    private readonly IGameTdbMetadataMapper _metadataMapper;

    public GameTdbUpdateSource(
        HttpClient httpClient,
        GameTdbMetadataProviderOptions options,
        IGameTdbMetadataMapper metadataMapper)
    {
        _httpClient = httpClient;
        _options = options;
        _metadataMapper = metadataMapper;
    }

    public async Task<GameTdbUpdateInfo?> GetLatestAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsHttps(_options.DatabaseUrl))
        {
            return null;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, _options.DatabaseUrl);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var publishedAt = response.Content.Headers.LastModified
                ?? response.Headers.Date
                ?? DateTimeOffset.UtcNow;
            var version = response.Headers.ETag?.Tag?.Trim('"');
            if (string.IsNullOrWhiteSpace(version))
            {
                version = publishedAt.UtcDateTime.ToString("yyyy.MM.dd.HHmmss", CultureInfo.InvariantCulture);
            }

            return new GameTdbUpdateInfo(version, publishedAt, _options.DatabaseUrl);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    public async Task<GameTdbUpdatePackage?> DownloadAsync(
        GameTdbUpdateInfo update,
        GameTdbUpdateOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsHttps(_options.DatabaseUrl))
        {
            return null;
        }

        Directory.CreateDirectory(GetTempDirectory());

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);

        var zipBytes = await DownloadZipAsync(timeout.Token);
        if (zipBytes is null)
        {
            return null;
        }

        var xml = ExtractXml(zipBytes);
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        var document = LoadXml(xml);
        var entries = ParseEntries(document);
        if (entries.Count < MinimumGameCount)
        {
            return null;
        }

        var downloadedAt = DateTimeOffset.UtcNow;
        var version = DetectSourceVersion(document) ?? update.Version;
        var metadata = entries
            .Select(entry => _metadataMapper.Map(entry, new GameTdbMetadataMappingContext(
                NormalizeLanguage(_options.PreferredLanguage),
                _options.FallbackLanguages,
                version,
                downloadedAt)))
            .ToArray();
        var zipSha256 = ComputeSha256(zipBytes);
        var xmlSha256 = ComputeSha256(xml);
        var manifest = new GameTdbUpdateManifest(
            ProviderName,
            _options.DatabaseUrl,
            downloadedAt,
            version,
            metadata.Length,
            xmlSha256,
            zipSha256,
            zipBytes.LongLength);

        await PersistDatabaseAsync(zipBytes, xml, manifest, timeout.Token);

        return new GameTdbUpdatePackage(
            update with { Version = version, PublishedAt = downloadedAt, Mirror = _options.DatabaseUrl },
            metadata,
            zipSha256);
    }

    private async Task<byte[]?> DownloadZipAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(_options.DatabaseUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await ReadLimitedAsync(stream, _options.MaxDatabaseZipBytes, cancellationToken);
    }

    private static string? ExtractXml(byte[] zipBytes)
    {
        using var memory = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(memory, ZipArchiveMode.Read);
        var entry = archive.Entries.FirstOrDefault(entry =>
            string.Equals(Path.GetFileName(entry.FullName), XmlFileName, StringComparison.OrdinalIgnoreCase));
        if (entry is null || entry.Length == 0 || IsUnsafeZipEntry(entry.FullName))
        {
            return null;
        }

        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static XDocument LoadXml(string xml)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var textReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(textReader, settings);
        return XDocument.Load(xmlReader, LoadOptions.None);
    }

    private static IReadOnlyList<GameTdbGameXmlEntry> ParseEntries(XDocument document)
    {
        return document
            .Descendants("game")
            .Select(ParseEntry)
            .Where(entry => !string.IsNullOrWhiteSpace(entry.GameId))
            .ToArray();
    }

    private static GameTdbGameXmlEntry ParseEntry(XElement game)
    {
        var gameId = ElementValue(game, "id") ?? AttributeValue(game, "id") ?? string.Empty;
        var date = game.Element("date");

        return new GameTdbGameXmlEntry(
            gameId.Trim().ToUpperInvariant(),
            AttributeValue(game, "name") ?? ElementValue(game, "name"),
            ElementValue(game, "region"),
            ElementValue(game, "developer"),
            ElementValue(game, "publisher"),
            date?.Value,
            AttributeValue(date, "year"),
            ElementValue(game, "languages"),
            ElementValue(game, "genre"),
            ElementValue(game.Element("input"), "players") ?? ElementValue(game, "players"),
            AttributeValue(game.Element("rating"), "value") ?? ElementValue(game, "rating"),
            ElementValue(game, "wi-fi"),
            ParseLocales(game));
    }

    private static IReadOnlyDictionary<string, GameTdbLocaleXmlEntry> ParseLocales(XElement game)
    {
        return game
            .Elements("locale")
            .Select(locale =>
            {
                var language = AttributeValue(locale, "lang") ?? AttributeValue(locale, "language") ?? "EN";
                return new GameTdbLocaleXmlEntry(
                    NormalizeLanguage(language),
                    ElementValue(locale, "title"),
                    ElementValue(locale, "synopsis"));
            })
            .GroupBy(locale => locale.Language, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    private async Task PersistDatabaseAsync(
        byte[] zipBytes,
        string xml,
        GameTdbUpdateManifest manifest,
        CancellationToken cancellationToken)
    {
        var currentDirectory = GetCurrentDirectory();
        var previousDirectory = GetPreviousDirectory();
        var tempDirectory = Path.Combine(GetTempDirectory(), Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path.GetDirectoryName(currentDirectory) ?? _options.DatabaseDirectory);
        Directory.CreateDirectory(tempDirectory);

        try
        {
            await File.WriteAllBytesAsync(Path.Combine(tempDirectory, ZipFileName), zipBytes, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, XmlFileName), xml, cancellationToken);
            await SaveManifestAsync(Path.Combine(tempDirectory, ManifestFileName), manifest, cancellationToken);

            if (Directory.Exists(previousDirectory))
            {
                Directory.Delete(previousDirectory, recursive: true);
            }

            if (Directory.Exists(currentDirectory))
            {
                Directory.Move(currentDirectory, previousDirectory);
            }

            Directory.Move(tempDirectory, currentDirectory);
            await WriteCompatibilityFilesAsync(zipBytes, xml, manifest, cancellationToken);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private async Task WriteCompatibilityFilesAsync(
        byte[] zipBytes,
        string xml,
        GameTdbUpdateManifest manifest,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.DatabaseDirectory);
        await File.WriteAllBytesAsync(Path.Combine(_options.DatabaseDirectory, ZipFileName), zipBytes, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(_options.DatabaseDirectory, XmlFileName), xml, cancellationToken);
        await SaveManifestAsync(Path.Combine(_options.DatabaseDirectory, ManifestFileName), new GameTdbDatabaseManifest(
            ProviderName,
            _options.DatabaseUrl,
            manifest.DownloadedAt,
            NormalizeLanguage(_options.PreferredLanguage),
            manifest.GameCount,
            manifest.Version), cancellationToken);
    }

    private static async Task SaveManifestAsync<T>(string path, T manifest, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken);
    }

    private static async Task<byte[]?> ReadLimitedAsync(Stream stream, long maxBytes, CancellationToken cancellationToken)
    {
        var memory = new MemoryStream();
        var buffer = new byte[81920];
        long totalBytes = 0;

        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                return memory.ToArray();
            }

            totalBytes += read;
            if (totalBytes > maxBytes)
            {
                return null;
            }

            memory.Write(buffer, 0, read);
        }
    }

    private static string ComputeSha256(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static string ComputeSha256(string text)
    {
        return ComputeSha256(System.Text.Encoding.UTF8.GetBytes(text));
    }

    private static string? DetectSourceVersion(XDocument document)
    {
        return AttributeValue(document.Root, "version")
            ?? AttributeValue(document.Root, "date")
            ?? AttributeValue(document.Root, "timestamp");
    }

    private string GetCurrentDirectory() => Path.Combine(_options.DatabaseDirectory, "current");

    private string GetPreviousDirectory() => Path.Combine(_options.DatabaseDirectory, "previous");

    private string GetTempDirectory() => Path.Combine(_options.DatabaseDirectory, "temp");

    private static string? ElementValue(XElement? element, string name)
    {
        return element?.Element(name)?.Value.Trim();
    }

    private static string? AttributeValue(XElement? element, string name)
    {
        return element?.Attribute(name)?.Value.Trim();
    }

    private static bool IsHttps(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }

    private static bool IsUnsafeZipEntry(string value)
    {
        return Path.IsPathRooted(value) || value.Split('/', '\\').Any(part => part == "..");
    }

    private static string NormalizeLanguage(string value)
    {
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
}
