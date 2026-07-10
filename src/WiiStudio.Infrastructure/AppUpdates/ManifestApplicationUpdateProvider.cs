using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.AppUpdates;

public sealed record ManifestApplicationUpdateProviderOptions
{
    public const string OfficialStableManifestUrl = "https://github.com/renatoaleixo/WiiStudio/releases/latest/download/wiistudio-update-manifest.stable.json";
    public const string OfficialBetaManifestUrl = "https://github.com/renatoaleixo/WiiStudio/releases/latest/download/wiistudio-update-manifest.beta.json";

    public string? ManifestUrl { get; init; }
    public string ApplicationId { get; init; } = "com.wiistudio.app";
    public string Channel { get; init; } = "stable";
    public string Platform { get; init; } = GetCurrentPlatform();
    public IReadOnlySet<string> TrustedHosts { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "github.com",
        "githubusercontent.com",
        "objects.githubusercontent.com"
    };

    public bool RequireSignature { get; init; }
    public string ManifestCachePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "updates",
        "manifest-cache.json");

    public string DownloadDirectory { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "updates",
        "downloads");

    private static string GetCurrentPlatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return "windows-x64";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "macos-x64";
        }

        return "linux-x64";
    }
}

public sealed class ManifestApplicationUpdateProvider : IApplicationUpdateProvider
{
    private readonly HttpClient httpClient;
    private readonly ManifestApplicationUpdateProviderOptions options;
    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
    private ApplicationUpdateManifestPackage? selectedPackage;

    public ManifestApplicationUpdateProvider(HttpClient httpClient, ManifestApplicationUpdateProviderOptions options)
    {
        this.httpClient = httpClient;
        this.options = options;
    }

    public async Task<ApplicationUpdateInfo?> CheckAsync(string currentVersion, CancellationToken cancellationToken)
    {
        if (!TryCreateTrustedHttpsUri(options.ManifestUrl, out var manifestUri))
        {
            return null;
        }

        var manifestJson = await GetTrustedStringAsync(manifestUri, cancellationToken);
        var manifest = JsonSerializer.Deserialize<ApplicationUpdateManifest>(manifestJson, jsonOptions);
        if (manifest is null || !IsManifestValid(manifest))
        {
            return null;
        }

        await CacheManifestAsync(manifestJson, cancellationToken);

        var latestVersion = manifest.ManifestVersion;
        var package = manifest.Packages.FirstOrDefault(item =>
            string.Equals(item.Platform, options.Platform, StringComparison.OrdinalIgnoreCase));
        if (package is null ||
            !TryCreateTrustedHttpsUri(package.Url, out _) ||
            string.IsNullOrWhiteSpace(package.Sha256) ||
            string.IsNullOrWhiteSpace(latestVersion))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(package.SignatureUrl) &&
            !TryCreateTrustedHttpsUri(package.SignatureUrl, out _))
        {
            return null;
        }

        if (options.RequireSignature && string.IsNullOrWhiteSpace(package.Signature) && string.IsNullOrWhiteSpace(package.SignatureUrl))
        {
            return null;
        }

        if (!IsUpdateAllowed(latestVersion, manifest.MinimumSupportedVersion, currentVersion))
        {
            return null;
        }

        selectedPackage = package;
        return new ApplicationUpdateInfo(
            latestVersion,
            currentVersion,
            IsAvailable: true,
            DateTimeOffset.UtcNow,
            package.Url,
            manifest.ReleaseNotes ?? manifest.ReleaseNotesUrl,
            package.Signature,
            package.Sha256);
    }

    public async Task<ApplicationUpdatePackage?> DownloadAsync(ApplicationUpdateInfo update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!TryCreateTrustedHttpsUri(update.DownloadUrl ?? selectedPackage?.Url, out var downloadUri))
        {
            throw new InvalidOperationException("Update package must use HTTPS and a trusted host.");
        }

        if (string.IsNullOrWhiteSpace(update.Checksum))
        {
            throw new InvalidOperationException("Update package checksum is required.");
        }

        if (options.RequireSignature && string.IsNullOrWhiteSpace(update.Signature) && string.IsNullOrWhiteSpace(selectedPackage?.SignatureUrl))
        {
            throw new InvalidOperationException("Update package signature is required.");
        }

        Directory.CreateDirectory(options.DownloadDirectory);
        var fileName = !string.IsNullOrWhiteSpace(selectedPackage?.FileName)
            ? selectedPackage.FileName
            : CreatePackageFileName(update.Version, downloadUri);
        var destination = Path.Combine(options.DownloadDirectory, fileName);

        await using (var source = await GetTrustedStreamAsync(downloadUri, cancellationToken))
        await using (var target = File.Create(destination))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        if (selectedPackage?.SizeBytes is > 0)
        {
            var size = new FileInfo(destination).Length;
            if (size != selectedPackage.SizeBytes)
            {
                File.Delete(destination);
                throw new InvalidOperationException("Downloaded update package size does not match the manifest.");
            }
        }

        var checksum = await ComputeSha256Async(destination, cancellationToken);
        if (!string.Equals(update.Checksum, checksum, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(destination);
            throw new InvalidOperationException("Downloaded update package checksum does not match the manifest.");
        }

        var signature = update.Signature;
        if (string.IsNullOrWhiteSpace(signature) &&
            TryCreateTrustedHttpsUri(selectedPackage?.SignatureUrl, out var signatureUri))
        {
            signature = await GetTrustedStringAsync(signatureUri, cancellationToken);
            await File.WriteAllTextAsync(destination + ".sig", signature, cancellationToken);
        }

        return new ApplicationUpdatePackage(update, destination, signature, checksum);
    }

    private async Task<string> GetTrustedStringAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        ValidateTrustedHttpsUri(response.RequestMessage?.RequestUri ?? uri);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<Stream> GetTrustedStreamAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        ValidateTrustedHttpsUri(response.RequestMessage?.RequestUri ?? uri);
        var memory = new MemoryStream();
        await response.Content.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;
        return memory;
    }

    private async Task CacheManifestAsync(string manifestJson, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(options.ManifestCachePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(options.ManifestCachePath, manifestJson, cancellationToken);
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private bool IsManifestValid(ApplicationUpdateManifest manifest)
    {
        if (manifest.SchemaVersion is not null and not 1)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(manifest.ApplicationId) &&
            !string.Equals(manifest.ApplicationId, options.ApplicationId, StringComparison.Ordinal))
        {
            return false;
        }

        return IsChannelAllowed(manifest.Channel);
    }

    private bool IsChannelAllowed(string channel)
    {
        if (string.Equals(options.Channel, "beta", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(channel, "beta", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(channel, "stable", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(options.Channel, "dev", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(channel, options.Channel, StringComparison.OrdinalIgnoreCase);
    }

    private bool TryCreateTrustedHttpsUri(string? value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out uri!) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            IsTrustedHost(uri))
        {
            return true;
        }

        uri = null!;
        return false;
    }

    private void ValidateTrustedHttpsUri(Uri uri)
    {
        if (uri.Scheme != Uri.UriSchemeHttps || !IsTrustedHost(uri))
        {
            throw new InvalidOperationException($"Update endpoint '{uri.Host}' is not trusted.");
        }
    }

    private bool IsTrustedHost(Uri uri) => options.TrustedHosts.Count == 0 || options.TrustedHosts.Contains(uri.Host);

    private static string CreatePackageFileName(string version, Uri downloadUri)
    {
        var extension = Path.GetExtension(downloadUri.LocalPath);
        return string.IsNullOrWhiteSpace(extension)
            ? $"WiiStudio-{version}.update"
            : $"WiiStudio-{version}{extension}";
    }

    private static bool IsUpdateAllowed(string latestVersion, string? minimumSupportedVersion, string currentVersion)
    {
        if (!string.IsNullOrWhiteSpace(minimumSupportedVersion) && CompareVersion(currentVersion, minimumSupportedVersion) < 0)
        {
            return true;
        }

        return CompareVersion(latestVersion, currentVersion) > 0;
    }

    private static bool IsNewer(string latestVersion, string currentVersion)
    {
        return CompareVersion(latestVersion, currentVersion) > 0;
    }

    private static int CompareVersion(string leftVersion, string rightVersion)
    {
        var left = ParseSemVer(leftVersion);
        var right = ParseSemVer(rightVersion);
        if (left is not null && right is not null)
        {
            for (var index = 0; index < left.Value.Numbers.Length; index++)
            {
                var comparison = left.Value.Numbers[index].CompareTo(right.Value.Numbers[index]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            if (left.Value.Prerelease is null && right.Value.Prerelease is not null)
            {
                return 1;
            }

            if (left.Value.Prerelease is not null && right.Value.Prerelease is null)
            {
                return -1;
            }

            return string.Compare(left.Value.Prerelease, right.Value.Prerelease, StringComparison.OrdinalIgnoreCase);
        }

        return string.Compare(leftVersion, rightVersion, StringComparison.OrdinalIgnoreCase);
    }

    private static ParsedVersion? ParseSemVer(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var withoutMetadata = value.Split('+', 2)[0];
        var versionParts = withoutMetadata.Split('-', 2);
        var numericParts = versionParts[0].Split('.');
        if (numericParts.Length is < 2 or > 3)
        {
            return null;
        }

        var numbers = new[] { 0, 0, 0 };
        for (var index = 0; index < numericParts.Length; index++)
        {
            if (!int.TryParse(numericParts[index], out numbers[index]))
            {
                return null;
            }
        }

        return new ParsedVersion(numbers, versionParts.Length > 1 ? versionParts[1] : null);
    }

    private readonly record struct ParsedVersion(int[] Numbers, string? Prerelease);
}

public sealed class StagedApplicationUpdateInstaller : IApplicationUpdateInstaller
{
    private readonly ApplicationUpdateStateStoreOptions options;

    public StagedApplicationUpdateInstaller(ApplicationUpdateStateStoreOptions options)
    {
        this.options = options;
    }

    public async Task InstallAsync(ApplicationUpdatePackage package, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (!File.Exists(package.PackagePath))
        {
            throw new FileNotFoundException("Validated update package was not found.", package.PackagePath);
        }

        var directory = Path.Combine(Path.GetDirectoryName(options.FilePath) ?? ".", "updates");
        Directory.CreateDirectory(directory);
        var marker = Path.Combine(directory, "pending-install.json");
        await File.WriteAllTextAsync(
            marker,
            $$"""
            {
              "version": "{{package.Info.Version}}",
              "packagePath": "{{JsonEscape(package.PackagePath)}}",
              "checksum": "{{package.Checksum}}",
              "createdAt": "{{DateTimeOffset.UtcNow:O}}"
            }
            """,
            cancellationToken);
    }

    public async Task RollbackAsync(ApplicationUpdateState state, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(Path.GetDirectoryName(options.FilePath) ?? ".", "updates");
        Directory.CreateDirectory(directory);
        var marker = Path.Combine(directory, "rollback-request.json");
        await File.WriteAllTextAsync(
            marker,
            $$"""
            {
              "fromVersion": "{{state.CurrentVersion}}",
              "toVersion": "{{state.PreviousVersion}}",
              "requestedAt": "{{DateTimeOffset.UtcNow:O}}"
            }
            """,
            cancellationToken);
    }

    private static string JsonEscape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}

public sealed record ApplicationUpdateManifest(
    [property: JsonPropertyName("schemaVersion")] int? SchemaVersion,
    [property: JsonPropertyName("applicationId")] string? ApplicationId,
    [property: JsonPropertyName("latestVersion")] string? LatestVersion,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("minimumSupportedVersion")] string? MinimumSupportedVersion,
    [property: JsonPropertyName("publishedAt")] DateTimeOffset? PublishedAt,
    [property: JsonPropertyName("releaseNotesUrl")] string? ReleaseNotesUrl,
    [property: JsonPropertyName("releaseNotes")] string? ReleaseNotes,
    [property: JsonPropertyName("packages")] IReadOnlyList<ApplicationUpdateManifestPackage> Packages)
{
    public string ManifestVersion => LatestVersion ?? Version;
}

public sealed record ApplicationUpdateManifestPackage(
    [property: JsonPropertyName("platform")] string Platform,
    [property: JsonPropertyName("kind")] string? Kind,
    [property: JsonPropertyName("fileName")] string? FileName,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("sha256")] string Sha256,
    [property: JsonPropertyName("signature")] string? Signature,
    [property: JsonPropertyName("signatureUrl")] string? SignatureUrl,
    [property: JsonPropertyName("signatureAlgorithm")] string? SignatureAlgorithm,
    [property: JsonPropertyName("signingKeyId")] string? SigningKeyId,
    [property: JsonPropertyName("sizeBytes")] long? SizeBytes);
