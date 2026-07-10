using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.AppUpdates;

public sealed record ManifestApplicationUpdateProviderOptions
{
    public string? ManifestUrl { get; init; }
    public string Channel { get; init; } = "stable";
    public string Platform { get; init; } = GetCurrentPlatform();
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
    private ApplicationUpdateManifestPackage? selectedPackage;

    public ManifestApplicationUpdateProvider(HttpClient httpClient, ManifestApplicationUpdateProviderOptions options)
    {
        this.httpClient = httpClient;
        this.options = options;
    }

    public async Task<ApplicationUpdateInfo?> CheckAsync(string currentVersion, CancellationToken cancellationToken)
    {
        if (!TryCreateHttpsUri(options.ManifestUrl, out var manifestUri))
        {
            return null;
        }

        var manifest = await httpClient.GetFromJsonAsync<ApplicationUpdateManifest>(manifestUri, cancellationToken);
        if (manifest is null || !string.Equals(manifest.Channel, options.Channel, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var package = manifest.Packages.FirstOrDefault(item =>
            string.Equals(item.Platform, options.Platform, StringComparison.OrdinalIgnoreCase));
        if (package is null || !TryCreateHttpsUri(package.Url, out _) || string.IsNullOrWhiteSpace(package.Sha256))
        {
            return null;
        }

        if (!IsNewer(manifest.Version, currentVersion))
        {
            return null;
        }

        selectedPackage = package;
        return new ApplicationUpdateInfo(
            manifest.Version,
            currentVersion,
            IsAvailable: true,
            DateTimeOffset.UtcNow,
            package.Url,
            manifest.ReleaseNotes,
            package.Signature,
            package.Sha256);
    }

    public async Task<ApplicationUpdatePackage?> DownloadAsync(ApplicationUpdateInfo update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!TryCreateHttpsUri(update.DownloadUrl ?? selectedPackage?.Url, out var downloadUri))
        {
            throw new InvalidOperationException("Update package must use HTTPS.");
        }

        if (string.IsNullOrWhiteSpace(update.Checksum))
        {
            throw new InvalidOperationException("Update package checksum is required.");
        }

        Directory.CreateDirectory(options.DownloadDirectory);
        var extension = Path.GetExtension(downloadUri.LocalPath);
        var fileName = string.IsNullOrWhiteSpace(extension)
            ? $"WiiStudio-{update.Version}.update"
            : $"WiiStudio-{update.Version}{extension}";
        var destination = Path.Combine(options.DownloadDirectory, fileName);

        await using (var source = await httpClient.GetStreamAsync(downloadUri, cancellationToken))
        await using (var target = File.Create(destination))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        var checksum = await ComputeSha256Async(destination, cancellationToken);
        if (!string.Equals(update.Checksum, checksum, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(destination);
            throw new InvalidOperationException("Downloaded update package checksum does not match the manifest.");
        }

        return new ApplicationUpdatePackage(update, destination, update.Signature, checksum);
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool TryCreateHttpsUri(string? value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out uri!) && uri.Scheme == Uri.UriSchemeHttps)
        {
            return true;
        }

        uri = null!;
        return false;
    }

    private static bool IsNewer(string latestVersion, string currentVersion)
    {
        if (Version.TryParse(latestVersion, out var latest) && Version.TryParse(currentVersion, out var current))
        {
            return latest > current;
        }

        return string.Compare(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
    }
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
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("releaseNotes")] string? ReleaseNotes,
    [property: JsonPropertyName("packages")] IReadOnlyList<ApplicationUpdateManifestPackage> Packages);

public sealed record ApplicationUpdateManifestPackage(
    [property: JsonPropertyName("platform")] string Platform,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("sha256")] string Sha256,
    [property: JsonPropertyName("signature")] string? Signature);
