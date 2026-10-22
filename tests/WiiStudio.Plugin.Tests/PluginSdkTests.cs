using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using WiiStudio.Plugin.Abstractions;
using WiiStudio.Plugin.Diagnostics;
using WiiStudio.Plugin.Hosting;
using WiiStudio.Plugin.Security;

namespace WiiStudio.Plugin.Tests;

public sealed class PluginSdkTests : IDisposable
{
    private readonly string tempRoot = Path.Combine(Path.GetTempPath(), "wiistudio-plugin-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Manifest_reader_parses_official_plugin_json_shape()
    {
        var manifestPath = CreatePluginManifest(
            "com.example.metadata",
            """
            {
              "id": "com.example.metadata",
              "name": "Example Metadata",
              "description": "Provides metadata.",
              "version": "1.0.0",
              "author": "Example",
              "entryAssembly": "Example.Metadata.dll",
              "entryType": "Example.Metadata.Plugin",
              "pluginSdkVersion": "1.0.0",
              "minimumWiiStudioVersion": "1.0.0",
              "supportedPlatforms": ["linux", "windows"],
              "capabilities": ["metadata-provider"],
              "permissions": ["access-metadata", "network-access"],
              "dependencies": [],
              "resources": {
                "icon": "assets/icon.png",
                "localization": "resources/i18n"
              }
            }
            """);

        var manifest = await new JsonPluginManifestReader().ReadAsync(manifestPath);

        manifest.Id.Should().Be("com.example.metadata");
        manifest.Capabilities.Should().ContainSingle().Which.Should().Be(PluginCapability.MetadataProvider);
        manifest.Permissions.Should().Contain([PluginPermission.AccessMetadata, PluginPermission.NetworkAccess]);
    }

    [Fact]
    public void Manifest_validator_blocks_paths_that_escape_plugin_directory()
    {
        var manifest = ValidManifest(entryAssembly: "../Bad.dll");
        var validator = new PluginManifestValidator(new PluginHostOptions());

        var result = validator.Validate(manifest, tempRoot);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("entryAssembly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Discovery_reads_manifests_without_loading_assemblies()
    {
        var installed = Path.Combine(tempRoot, "installed");
        var pluginDirectory = Path.Combine(installed, "example");
        Directory.CreateDirectory(pluginDirectory);
        await File.WriteAllTextAsync(Path.Combine(pluginDirectory, "plugin.json"), ValidManifestJson("com.example.ready"));

        var diagnostics = new PluginDiagnosticService();
        var registry = new PluginRegistry();
        var service = new FileSystemPluginDiscoveryService(
            new PluginHostOptions { InstalledDirectory = installed },
            new JsonPluginManifestReader(),
            new PluginManifestValidator(new PluginHostOptions()),
            registry,
            diagnostics);

        var descriptors = await service.DiscoverAsync();

        descriptors.Should().ContainSingle();
        descriptors[0].State.Should().Be(PluginDiscoveryState.ReadyForLoad);
        registry.Descriptors.Should().ContainSingle();
    }

    [Fact]
    public async Task Discovery_marks_duplicate_plugins()
    {
        var installed = Path.Combine(tempRoot, "installed");
        Directory.CreateDirectory(Path.Combine(installed, "one"));
        Directory.CreateDirectory(Path.Combine(installed, "two"));
        await File.WriteAllTextAsync(Path.Combine(installed, "one", "plugin.json"), ValidManifestJson("com.example.duplicate"));
        await File.WriteAllTextAsync(Path.Combine(installed, "two", "plugin.json"), ValidManifestJson("com.example.duplicate"));

        var service = new FileSystemPluginDiscoveryService(
            new PluginHostOptions { InstalledDirectory = installed },
            new JsonPluginManifestReader(),
            new PluginManifestValidator(new PluginHostOptions()),
            new PluginRegistry(),
            new PluginDiagnosticService());

        var descriptors = await service.DiscoverAsync();

        descriptors.Should().HaveCount(2);
        descriptors.Should().OnlyContain(item => item.State == PluginDiscoveryState.Duplicate);
    }

    [Fact]
    public async Task Permission_service_never_grants_sensitive_permission_implicitly()
    {
        var service = new PluginPermissionService();

        var decision = await service.RequestPermissionAsync("com.example.converter", PluginPermission.ExecuteProcess);

        decision.Should().Be(PermissionDecision.RequiresUserApproval);
        service.HasPermission("com.example.converter", PluginPermission.ExecuteProcess).Should().BeFalse();
    }

    [Fact]
    public async Task Loader_fails_safely_when_entry_assembly_is_missing()
    {
        var descriptor = new PluginDescriptor(
            "com.example.missing",
            "Missing",
            "1.0.0",
            tempRoot,
            Path.Combine(tempRoot, "plugin.json"),
            ValidManifest(),
            PluginDiscoveryState.ReadyForLoad,
            [],
            []);

        var loader = new PluginLoaderService(
            new PluginHostOptions(),
            new PluginManifestValidator(new PluginHostOptions()),
            new PluginPermissionService(),
            new PluginRegistry(),
            new PluginDiagnosticService(),
            new EmptyServiceProvider(),
            new FileSystemPluginStorage(Path.Combine(tempRoot, "storage")),
            new NullPluginNotificationService());

        var result = await loader.LoadAsync(descriptor);

        result.Success.Should().BeFalse();
        result.State.Should().Be(PluginLoadState.Failed);
        result.Errors.Should().Contain(error => error.Contains("Assembly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Plugin_storage_blocks_path_traversal()
    {
        var storage = new FileSystemPluginStorage(Path.Combine(tempRoot, "storage"));

        var act = async () => await storage.OpenWriteAsync("com.example.storage", "../escape.txt");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Plugin_management_installs_local_directory_without_loading_assembly()
    {
        var sourceDirectory = Path.Combine(tempRoot, "source");
        Directory.CreateDirectory(sourceDirectory);
        await File.WriteAllTextAsync(Path.Combine(sourceDirectory, "plugin.json"), ValidManifestJson("com.example.install"));
        var service = CreateManagementService();

        var result = await service.InstallLocalAsync(sourceDirectory);

        result.Success.Should().BeTrue();
        result.Plugin.Should().NotBeNull();
        result.Plugin!.Id.Should().Be("com.example.install");
        result.Plugin.State.Should().Be(PluginDiscoveryState.ReadyForLoad);
        File.Exists(Path.Combine(tempRoot, "installed", "com.example.install", "plugin.json")).Should().BeTrue();
    }

    [Fact]
    public async Task Plugin_management_can_disable_and_enable_plugin()
    {
        var installedDirectory = Path.Combine(tempRoot, "installed", "com.example.toggle");
        Directory.CreateDirectory(installedDirectory);
        await File.WriteAllTextAsync(Path.Combine(installedDirectory, "plugin.json"), ValidManifestJson("com.example.toggle"));
        var service = CreateManagementService();

        var disabled = await service.DisableAsync("com.example.toggle");
        var enabled = await service.EnableAsync("com.example.toggle");

        disabled.Success.Should().BeTrue();
        enabled.Success.Should().BeTrue();
        File.Exists(Path.Combine(tempRoot, "installed", "com.example.toggle", "plugin.json")).Should().BeTrue();
        Directory.Exists(Path.Combine(tempRoot, "disabled", "com.example.toggle")).Should().BeFalse();
    }

    [Fact]
    public async Task Remote_catalog_installs_valid_plugin_disabled_by_default()
    {
        var packageBytes = CreatePluginPackage("com.example.remote");
        var checksum = Convert.ToHexString(SHA256.HashData(packageBytes)).ToLowerInvariant();
        var http = new FakeHttpMessageHandler();
        http.AddJson("https://plugins.example/catalog.json", $$"""
        {
          "schemaVersion": 1,
          "publishedAt": "2026-01-01T10:00:00Z",
          "catalogVersion": "1.0.0",
          "plugins": [
            {
              "pluginId": "com.example.remote",
              "name": "Remote Example",
              "description": "Remote plugin.",
              "author": "Example",
              "version": "1.0.0",
              "minWiiStudioVersion": "1.0.0",
              "packageUrl": "https://plugins.example/com.example.remote.zip",
              "sha256": "{{checksum}}",
              "permissions": ["AccessMetadata"],
              "riskLevel": "Low",
              "trustLevel": "Community",
              "status": "Active"
            }
          ],
          "revocations": []
        }
        """);
        http.AddBytes("https://plugins.example/com.example.remote.zip", packageBytes);
        var service = CreateManagementService(new PluginHostOptions
        {
            InstalledDirectory = Path.Combine(tempRoot, "installed"),
            DisabledDirectory = Path.Combine(tempRoot, "disabled"),
            QuarantineDirectory = Path.Combine(tempRoot, "quarantine"),
            DownloadDirectory = Path.Combine(tempRoot, "downloads"),
            CatalogCachePath = Path.Combine(tempRoot, "catalog-cache.json"),
            RemoteCatalogUrl = "https://plugins.example/catalog.json",
            TrustedCatalogHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "plugins.example" }
        }, new HttpClient(http));

        var result = await service.InstallRemoteAsync("com.example.remote");

        result.Success.Should().BeTrue();
        result.Plugin.Should().NotBeNull();
        result.Plugin!.State.Should().Be(PluginDiscoveryState.Disabled);
        File.Exists(Path.Combine(tempRoot, "disabled", "com.example.remote", "plugin.json")).Should().BeTrue();
        File.Exists(Path.Combine(tempRoot, "catalog-cache.json")).Should().BeTrue();
    }

    [Fact]
    public async Task Remote_install_rejects_archive_entry_outside_extraction_directory()
    {
        var escapedFileName = $"wiistudio-zip-slip-{Guid.NewGuid():N}.txt";
        var escapedPath = Path.Combine(Path.GetTempPath(), "wiistudio-plugin-remote", escapedFileName);
        byte[] packageBytes;
        using (var memory = new MemoryStream())
        {
            using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = archive.CreateEntry($"../{escapedFileName}");
                await using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                await writer.WriteAsync("must not escape");
            }

            packageBytes = memory.ToArray();
        }

        var checksum = Convert.ToHexString(SHA256.HashData(packageBytes)).ToLowerInvariant();
        var http = new FakeHttpMessageHandler();
        http.AddJson("https://plugins.example/catalog.json", $$"""
        {
          "schemaVersion": 1,
          "plugins": [
            {
              "pluginId": "com.example.zipslip",
              "name": "Unsafe Archive",
              "description": "Traversal test.",
              "author": "Example",
              "version": "1.0.0",
              "minWiiStudioVersion": "1.0.0",
              "packageUrl": "https://plugins.example/com.example.zipslip.zip",
              "sha256": "{{checksum}}",
              "permissions": ["AccessMetadata"],
              "riskLevel": "Low",
              "trustLevel": "Community",
              "status": "Active"
            }
          ],
          "revocations": []
        }
        """);
        http.AddBytes("https://plugins.example/com.example.zipslip.zip", packageBytes);
        var service = CreateRemoteManagementService(http);

        try
        {
            var result = await service.InstallRemoteAsync("com.example.zipslip");

            result.Success.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Contains("caminho inseguro", StringComparison.OrdinalIgnoreCase));
            File.Exists(escapedPath).Should().BeFalse();
        }
        finally
        {
            File.Delete(escapedPath);
        }
    }

    [Fact]
    public async Task Remote_catalog_rejects_http_package_url()
    {
        var http = new FakeHttpMessageHandler();
        http.AddJson("https://plugins.example/catalog.json", """
        {
          "schemaVersion": 1,
          "plugins": [
            {
              "pluginId": "com.example.bad",
              "name": "Bad",
              "description": "Bad plugin.",
              "author": "Example",
              "version": "1.0.0",
              "minWiiStudioVersion": "1.0.0",
              "packageUrl": "http://plugins.example/com.example.bad.zip",
              "sha256": "checksum",
              "permissions": ["AccessMetadata"],
              "riskLevel": "Low",
              "trustLevel": "Community",
              "status": "Active"
            }
          ],
          "revocations": []
        }
        """);
        var service = CreateRemoteManagementService(http);

        var catalog = await service.RefreshRemoteCatalogAsync();

        catalog.Success.Should().BeFalse();
        catalog.Errors.Should().Contain(error => error.Contains("packageUrl", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Remote_install_quarantines_invalid_checksum_package()
    {
        var packageBytes = CreatePluginPackage("com.example.badchecksum");
        var http = new FakeHttpMessageHandler();
        http.AddJson("https://plugins.example/catalog.json", """
        {
          "schemaVersion": 1,
          "plugins": [
            {
              "pluginId": "com.example.badchecksum",
              "name": "Bad Checksum",
              "description": "Bad checksum plugin.",
              "author": "Example",
              "version": "1.0.0",
              "minWiiStudioVersion": "1.0.0",
              "packageUrl": "https://plugins.example/com.example.badchecksum.zip",
              "sha256": "0000000000000000000000000000000000000000000000000000000000000000",
              "permissions": ["AccessMetadata"],
              "riskLevel": "Low",
              "trustLevel": "Community",
              "status": "Active"
            }
          ],
          "revocations": []
        }
        """);
        http.AddBytes("https://plugins.example/com.example.badchecksum.zip", packageBytes);
        var service = CreateRemoteManagementService(http);

        var result = await service.InstallRemoteAsync("com.example.badchecksum");

        result.Success.Should().BeFalse();
        result.QuarantinePath.Should().NotBeNull();
        Directory.Exists(result.QuarantinePath!).Should().BeTrue();
    }

    [Fact]
    public async Task Remote_revocation_moves_installed_plugin_to_quarantine()
    {
        var installedDirectory = Path.Combine(tempRoot, "installed", "com.example.revoked");
        Directory.CreateDirectory(installedDirectory);
        await File.WriteAllTextAsync(Path.Combine(installedDirectory, "plugin.json"), ValidManifestJson("com.example.revoked"));
        var http = new FakeHttpMessageHandler();
        http.AddJson("https://plugins.example/catalog.json", """
        {
          "schemaVersion": 1,
          "plugins": [],
          "revocations": [
            {
              "pluginId": "com.example.revoked",
              "reason": "Unsafe behavior",
              "revokedAt": "2026-01-01T10:00:00Z"
            }
          ]
        }
        """);
        var service = CreateRemoteManagementService(http);

        var results = await service.ApplyRemoteRevocationsAsync();

        results.Should().ContainSingle(item => item.PluginId == "com.example.revoked" && item.Applied);
        Directory.Exists(installedDirectory).Should().BeFalse();
        Directory.EnumerateFiles(Path.Combine(tempRoot, "quarantine"), "plugin.json", SearchOption.AllDirectories)
            .Should().ContainSingle();
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private string CreatePluginManifest(string pluginId, string json)
    {
        var pluginDirectory = Path.Combine(tempRoot, pluginId);
        Directory.CreateDirectory(pluginDirectory);
        var manifestPath = Path.Combine(pluginDirectory, "plugin.json");
        File.WriteAllText(manifestPath, json);
        return manifestPath;
    }

    private static PluginManifest ValidManifest(string entryAssembly = "Example.Plugin.dll") =>
        new(
            "com.example.valid",
            "Example",
            "Example plugin.",
            "1.0.0",
            "Example",
            null,
            null,
            null,
            entryAssembly,
            "Example.Plugin",
            "1.0.0",
            "1.0.0",
            null,
            [],
            [PluginCapability.MetadataProvider],
            [PluginPermission.AccessMetadata],
            [],
            new PluginResources(null, null),
            [],
            null,
            null);

    private static string ValidManifestJson(string pluginId) =>
        $$"""
        {
          "id": "{{pluginId}}",
          "name": "Example",
          "description": "Example plugin.",
          "version": "1.0.0",
          "author": "Example",
          "entryAssembly": "Example.Plugin.dll",
          "entryType": "Example.Plugin",
          "pluginSdkVersion": "1.0.0",
          "minimumWiiStudioVersion": "1.0.0",
          "supportedPlatforms": [],
          "capabilities": ["metadata-provider"],
          "permissions": ["access-metadata"],
          "dependencies": []
        }
        """;

    private IPluginManagementService CreateManagementService()
    {
        return CreateManagementService(new PluginHostOptions
        {
            InstalledDirectory = Path.Combine(tempRoot, "installed"),
            DisabledDirectory = Path.Combine(tempRoot, "disabled"),
            QuarantineDirectory = Path.Combine(tempRoot, "quarantine")
        });
    }

    private IPluginManagementService CreateRemoteManagementService(FakeHttpMessageHandler http)
    {
        return CreateManagementService(new PluginHostOptions
        {
            InstalledDirectory = Path.Combine(tempRoot, "installed"),
            DisabledDirectory = Path.Combine(tempRoot, "disabled"),
            QuarantineDirectory = Path.Combine(tempRoot, "quarantine"),
            DownloadDirectory = Path.Combine(tempRoot, "downloads"),
            CatalogCachePath = Path.Combine(tempRoot, "catalog-cache.json"),
            RemoteCatalogUrl = "https://plugins.example/catalog.json",
            TrustedCatalogHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "plugins.example" }
        }, new HttpClient(http));
    }

    private IPluginManagementService CreateManagementService(PluginHostOptions options, HttpClient? httpClient = null)
    {
        var diagnostics = new PluginDiagnosticService();
        var registry = new PluginRegistry();
        var reader = new JsonPluginManifestReader();
        var validator = new PluginManifestValidator(options);
        var permissions = new PluginPermissionService();
        var discovery = new FileSystemPluginDiscoveryService(options, reader, validator, registry, diagnostics);
        var loader = new PluginLoaderService(
            options,
            validator,
            permissions,
            registry,
            diagnostics,
            new EmptyServiceProvider(),
            new FileSystemPluginStorage(Path.Combine(tempRoot, "storage")),
            new NullPluginNotificationService());

        return new FileSystemPluginManagementService(
            options,
            discovery,
            reader,
            validator,
            loader,
            diagnostics,
            permissions,
            new DefaultPluginPermissionPolicy(),
            httpClient);
    }

    private static byte[] CreatePluginPackage(string pluginId)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifest = archive.CreateEntry("plugin/plugin.json");
            using var writer = new StreamWriter(manifest.Open(), Encoding.UTF8);
            writer.Write(ValidManifestJson(pluginId));
        }

        return memory.ToArray();
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, Func<HttpResponseMessage>> responses = new(StringComparer.OrdinalIgnoreCase);

        public void AddJson(string url, string json) =>
            responses[url] = () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

        public void AddBytes(string url, byte[] bytes) =>
            responses[url] = () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is not null && responses.TryGetValue(request.RequestUri.ToString(), out var responseFactory))
            {
                return Task.FromResult(responseFactory());
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
