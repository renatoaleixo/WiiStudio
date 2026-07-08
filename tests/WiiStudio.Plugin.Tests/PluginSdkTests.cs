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

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
