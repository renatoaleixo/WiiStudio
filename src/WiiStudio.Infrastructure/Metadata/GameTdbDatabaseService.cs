using System.IO.Compression;
using System.Text.Json;

namespace WiiStudio.Infrastructure.Metadata;

public sealed class GameTdbDatabaseService : IGameTdbDatabaseService
{
    private const string ZipFileName = "wiitdb.zip";
    private const string XmlFileName = "wiitdb.xml";
    private const string ManifestFileName = "manifest.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly HttpClient _httpClient;
    private readonly GameTdbMetadataProviderOptions _options;

    public GameTdbDatabaseService(HttpClient httpClient, GameTdbMetadataProviderOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<GameTdbDatabaseStatus> EnsureDatabaseAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var xmlPath = GetXmlPath();
        var manifest = await LoadManifestAsync(cancellationToken);
        if (!forceRefresh && File.Exists(xmlPath) && manifest is not null && !IsExpired(manifest.DownloadedAt))
        {
            return new GameTdbDatabaseStatus(true, xmlPath, manifest.DownloadedAt, manifest.SourceVersion, "GameTDB database is available from local cache.");
        }

        if (!_options.AllowAutomaticMetadataDownload)
        {
            return new GameTdbDatabaseStatus(File.Exists(xmlPath), File.Exists(xmlPath) ? xmlPath : null, manifest?.DownloadedAt, manifest?.SourceVersion, "Automatic metadata download is disabled.");
        }

        if (!IsHttps(_options.DatabaseUrl))
        {
            return new GameTdbDatabaseStatus(File.Exists(xmlPath), File.Exists(xmlPath) ? xmlPath : null, manifest?.DownloadedAt, manifest?.SourceVersion, "GameTDB metadata database URL must use HTTPS.");
        }

        Directory.CreateDirectory(_options.DatabaseDirectory);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);

        try
        {
            var zipBytes = await DownloadZipAsync(timeout.Token);
            if (zipBytes is null)
            {
                return new GameTdbDatabaseStatus(File.Exists(xmlPath), File.Exists(xmlPath) ? xmlPath : null, manifest?.DownloadedAt, manifest?.SourceVersion, "GameTDB database could not be downloaded.");
            }

            var extractedXml = ExtractXml(zipBytes);
            if (string.IsNullOrWhiteSpace(extractedXml))
            {
                return new GameTdbDatabaseStatus(File.Exists(xmlPath), File.Exists(xmlPath) ? xmlPath : null, manifest?.DownloadedAt, manifest?.SourceVersion, "GameTDB database ZIP does not contain a valid wiitdb.xml.");
            }

            await WriteDatabaseAsync(zipBytes, extractedXml, timeout.Token);
            var downloadedAt = DateTimeOffset.UtcNow;
            var newManifest = new GameTdbDatabaseManifest(
                "GameTDB",
                _options.DatabaseUrl,
                downloadedAt,
                NormalizeLanguage(_options.PreferredLanguage),
                CountGames(extractedXml),
                DetectSourceVersion(extractedXml) ?? downloadedAt.ToString("O"));
            await SaveManifestAsync(newManifest, timeout.Token);

            return new GameTdbDatabaseStatus(true, xmlPath, newManifest.DownloadedAt, newManifest.SourceVersion, "GameTDB database downloaded.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new GameTdbDatabaseStatus(File.Exists(xmlPath), File.Exists(xmlPath) ? xmlPath : null, manifest?.DownloadedAt, manifest?.SourceVersion, "GameTDB database download timed out.");
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or InvalidDataException or JsonException)
        {
            return new GameTdbDatabaseStatus(File.Exists(xmlPath), File.Exists(xmlPath) ? xmlPath : null, manifest?.DownloadedAt, manifest?.SourceVersion, exception.Message);
        }
    }

    public Task<Stream> OpenDatabaseXmlAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<Stream>(File.OpenRead(GetXmlPath()));
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

    private async Task WriteDatabaseAsync(byte[] zipBytes, string xml, CancellationToken cancellationToken)
    {
        var zipPath = GetZipPath();
        var xmlPath = GetXmlPath();
        var tempZipPath = $"{zipPath}.{Guid.NewGuid():N}.tmp";
        var tempXmlPath = $"{xmlPath}.{Guid.NewGuid():N}.tmp";

        try
        {
            await File.WriteAllBytesAsync(tempZipPath, zipBytes, cancellationToken);
            await File.WriteAllTextAsync(tempXmlPath, xml, cancellationToken);
            File.Move(tempZipPath, zipPath, overwrite: true);
            File.Move(tempXmlPath, xmlPath, overwrite: true);
        }
        finally
        {
            DeleteIfExists(tempZipPath);
            DeleteIfExists(tempXmlPath);
        }
    }

    private async Task<GameTdbDatabaseManifest?> LoadManifestAsync(CancellationToken cancellationToken)
    {
        var path = GetManifestPath();
        if (!File.Exists(path))
        {
            return null;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<GameTdbDatabaseManifest>(stream, JsonOptions, cancellationToken);
    }

    private async Task SaveManifestAsync(GameTdbDatabaseManifest manifest, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(GetManifestPath());
        await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken);
    }

    private bool IsExpired(DateTimeOffset downloadedAt)
    {
        return downloadedAt.Add(_options.DatabaseTtl) <= DateTimeOffset.UtcNow;
    }

    private string GetZipPath() => Path.Combine(_options.DatabaseDirectory, ZipFileName);

    private string GetXmlPath() => Path.Combine(_options.DatabaseDirectory, XmlFileName);

    private string GetManifestPath() => Path.Combine(_options.DatabaseDirectory, ManifestFileName);

    private static bool IsHttps(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }

    private static bool IsUnsafeZipEntry(string value)
    {
        return Path.IsPathRooted(value) ||
            value.Split('/', '\\').Any(part => part == "..");
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

    private static int CountGames(string xml)
    {
        var count = 0;
        var index = 0;
        while ((index = xml.IndexOf("<game", index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += 5;
        }

        return count;
    }

    private static string? DetectSourceVersion(string xml)
    {
        const string versionAttribute = "version=\"";
        var datafileIndex = xml.IndexOf("<datafile", StringComparison.OrdinalIgnoreCase);
        var index = datafileIndex < 0
            ? xml.IndexOf(versionAttribute, StringComparison.OrdinalIgnoreCase)
            : xml.IndexOf(versionAttribute, datafileIndex, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var start = index + versionAttribute.Length;
        var end = xml.IndexOf('"', start);
        return end > start ? xml[start..end] : null;
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

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
