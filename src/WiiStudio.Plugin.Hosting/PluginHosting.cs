using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Plugin.Abstractions;
using WiiStudio.Plugin.Diagnostics;

namespace WiiStudio.Plugin.Hosting;

public sealed record PluginHostOptions
{
    public string InstalledDirectory { get; init; } = Path.Combine("plugins", "installed");
    public string DisabledDirectory { get; init; } = Path.Combine("plugins", "disabled");
    public string QuarantineDirectory { get; init; } = Path.Combine("plugins", "quarantine");
    public string CurrentWiiStudioVersion { get; init; } = "1.0.0";
    public string CurrentPluginSdkVersion { get; init; } = "1.0.0";
    public TimeSpan InitializeTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan ShutdownTimeout { get; init; } = TimeSpan.FromSeconds(10);
}

public interface IPluginManifestReader
{
    Task<PluginManifest> ReadAsync(string manifestPath, CancellationToken cancellationToken = default);
}

public interface IPluginManifestValidator
{
    PluginValidationResult Validate(PluginManifest manifest, string pluginDirectory);
}

public interface IPluginDiscoveryService
{
    Task<IReadOnlyList<PluginDescriptor>> DiscoverAsync(CancellationToken cancellationToken = default);
}

public interface IPluginLoaderService
{
    Task<PluginLoadResult> LoadAsync(PluginDescriptor descriptor, CancellationToken cancellationToken = default);
    Task UnloadAsync(string pluginId, CancellationToken cancellationToken = default);
}

public interface IPluginRegistry
{
    IReadOnlyList<PluginDescriptor> Descriptors { get; }
    IReadOnlyList<IWiiStudioPlugin> RunningPlugins { get; }
    bool TryGetDescriptor(string pluginId, out PluginDescriptor descriptor);
    bool TryGetRunningPlugin(string pluginId, out IWiiStudioPlugin plugin);
    void RegisterDescriptor(PluginDescriptor descriptor);
    void RegisterRunningPlugin(PluginDescriptor descriptor, IWiiStudioPlugin plugin, AssemblyLoadContext loadContext);
    bool Remove(string pluginId, out IWiiStudioPlugin? plugin, out AssemblyLoadContext? loadContext);
}

public sealed class JsonPluginManifestReader : IPluginManifestReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public async Task<PluginManifest> ReadAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(manifestPath);
        var dto = await JsonSerializer.DeserializeAsync<PluginManifestDto>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("plugin.json vazio ou inválido.");

        return dto.ToManifest();
    }
}

public sealed class PluginManifestValidator : IPluginManifestValidator
{
    private readonly PluginHostOptions options;

    public PluginManifestValidator(PluginHostOptions options)
    {
        this.options = options;
    }

    public PluginValidationResult Validate(PluginManifest manifest, string pluginDirectory)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        Require(manifest.Id, "id", errors);
        Require(manifest.Name, "name", errors);
        Require(manifest.Version, "version", errors);
        Require(manifest.EntryAssembly, "entryAssembly", errors);
        Require(manifest.EntryType, "entryType", errors);
        Require(manifest.PluginSdkVersion, "pluginSdkVersion", errors);
        Require(manifest.MinimumWiiStudioVersion, "minimumWiiStudioVersion", errors);

        if (manifest.Capabilities.Count == 0)
        {
            errors.Add("capabilities deve declarar ao menos uma capacidade.");
        }

        if (!Version.TryParse(RemovePrerelease(manifest.Version), out _))
        {
            errors.Add("version deve seguir Semantic Versioning.");
        }

        if (!Version.TryParse(RemovePrerelease(manifest.PluginSdkVersion), out var sdkVersion))
        {
            errors.Add("pluginSdkVersion inválida.");
        }
        else if (Version.TryParse(options.CurrentPluginSdkVersion, out var currentSdkVersion) && sdkVersion.Major > currentSdkVersion.Major)
        {
            errors.Add("pluginSdkVersion é incompatível com a versão atual do SDK.");
        }

        if (Version.TryParse(RemovePrerelease(manifest.MinimumWiiStudioVersion), out var minimumVersion)
            && Version.TryParse(options.CurrentWiiStudioVersion, out var currentVersion)
            && minimumVersion > currentVersion)
        {
            errors.Add("minimumWiiStudioVersion é maior que a versão atual do WiiStudio.");
        }

        if (!string.IsNullOrWhiteSpace(manifest.MaximumWiiStudioVersion)
            && Version.TryParse(RemovePrerelease(manifest.MaximumWiiStudioVersion), out var maximumVersion)
            && Version.TryParse(options.CurrentWiiStudioVersion, out var currentForMaximum)
            && maximumVersion < currentForMaximum)
        {
            warnings.Add("maximumWiiStudioVersion é menor que a versão atual do WiiStudio.");
        }

        if (!IsSafeRelativeAssemblyPath(pluginDirectory, manifest.EntryAssembly))
        {
            errors.Add("entryAssembly deve ser um caminho relativo seguro dentro da pasta do plugin.");
        }

        if (manifest.SupportedPlatforms.Count > 0 && !IsCurrentPlatformSupported(manifest.SupportedPlatforms))
        {
            errors.Add("supportedPlatforms não inclui a plataforma atual.");
        }

        return errors.Count == 0
            ? new PluginValidationResult(true, PluginDiscoveryState.ReadyForLoad, errors, warnings)
            : new PluginValidationResult(false, PluginDiscoveryState.InvalidManifest, errors, warnings);
    }

    private static void Require(string value, string fieldName, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} é obrigatório.");
        }
    }

    private static string RemovePrerelease(string version)
    {
        var index = version.IndexOf('-', StringComparison.Ordinal);
        return index >= 0 ? version[..index] : version;
    }

    private static bool IsSafeRelativeAssemblyPath(string pluginDirectory, string entryAssembly)
    {
        if (string.IsNullOrWhiteSpace(entryAssembly) || Path.IsPathRooted(entryAssembly) || Path.GetExtension(entryAssembly) != ".dll")
        {
            return false;
        }

        var root = Path.GetFullPath(pluginDirectory);
        var target = Path.GetFullPath(Path.Combine(root, entryAssembly));
        return target.StartsWith(root, StringComparison.Ordinal);
    }

    private static bool IsCurrentPlatformSupported(IEnumerable<string> platforms)
    {
        var current = OperatingSystem.IsWindows()
            ? "windows"
            : OperatingSystem.IsLinux()
                ? "linux"
                : OperatingSystem.IsMacOS()
                    ? "macos"
                    : "unknown";

        return platforms.Any(platform => string.Equals(platform, current, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class FileSystemPluginDiscoveryService : IPluginDiscoveryService
{
    private readonly PluginHostOptions options;
    private readonly IPluginManifestReader reader;
    private readonly IPluginManifestValidator validator;
    private readonly IPluginRegistry registry;
    private readonly IPluginDiagnosticService diagnostics;

    public FileSystemPluginDiscoveryService(
        PluginHostOptions options,
        IPluginManifestReader reader,
        IPluginManifestValidator validator,
        IPluginRegistry registry,
        IPluginDiagnosticService diagnostics)
    {
        this.options = options;
        this.reader = reader;
        this.validator = validator;
        this.registry = registry;
        this.diagnostics = diagnostics;
    }

    public async Task<IReadOnlyList<PluginDescriptor>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        var descriptors = new List<PluginDescriptor>();
        descriptors.AddRange(await DiscoverInDirectoryAsync(options.InstalledDirectory, PluginDiscoveryState.Found, cancellationToken));
        descriptors.AddRange(await DiscoverInDirectoryAsync(options.DisabledDirectory, PluginDiscoveryState.Disabled, cancellationToken));
        descriptors.AddRange(await DiscoverInDirectoryAsync(options.QuarantineDirectory, PluginDiscoveryState.Quarantined, cancellationToken));

        foreach (var duplicateGroup in descriptors.GroupBy(item => item.Id, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
        {
            foreach (var descriptor in duplicateGroup)
            {
                var duplicate = descriptor with
                {
                    State = PluginDiscoveryState.Duplicate,
                    Errors = descriptor.Errors.Concat(["Plugin duplicado encontrado durante discovery."]).ToArray()
                };

                descriptors[descriptors.IndexOf(descriptor)] = duplicate;
                diagnostics.Record(CreateDiagnostic(duplicate.Id, "discovery.duplicate", DiagnosticSeverity.Error, "plugin.discovery.duplicate"));
            }
        }

        foreach (var descriptor in descriptors.Where(item => item.State != PluginDiscoveryState.Duplicate))
        {
            registry.RegisterDescriptor(descriptor);
        }

        return descriptors;
    }

    private async Task<IReadOnlyList<PluginDescriptor>> DiscoverInDirectoryAsync(
        string directory,
        PluginDiscoveryState baseState,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        var descriptors = new List<PluginDescriptor>();
        foreach (var manifestPath in Directory.EnumerateFiles(directory, "plugin.json", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pluginDirectory = Path.GetDirectoryName(manifestPath) ?? directory;
            try
            {
                var manifest = await reader.ReadAsync(manifestPath, cancellationToken);
                var validation = baseState is PluginDiscoveryState.Disabled or PluginDiscoveryState.Quarantined
                    ? new PluginValidationResult(true, baseState, [], [])
                    : validator.Validate(manifest, pluginDirectory);

                var state = baseState is PluginDiscoveryState.Disabled or PluginDiscoveryState.Quarantined
                    ? baseState
                    : validation.State;

                var descriptor = new PluginDescriptor(
                    manifest.Id,
                    manifest.Name,
                    manifest.Version,
                    pluginDirectory,
                    manifestPath,
                    manifest,
                    state,
                    validation.Warnings,
                    validation.Errors);

                descriptors.Add(descriptor);
                diagnostics.Record(CreateDiagnostic(manifest.Id, "discovery.manifest", DiagnosticSeverity.Information, "plugin.discovery.manifest"));
            }
            catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException)
            {
                var descriptor = new PluginDescriptor(
                    Path.GetFileName(pluginDirectory),
                    Path.GetFileName(pluginDirectory),
                    string.Empty,
                    pluginDirectory,
                    manifestPath,
                    EmptyManifest(Path.GetFileName(pluginDirectory)),
                    PluginDiscoveryState.InvalidManifest,
                    [],
                    [ex.Message]);

                descriptors.Add(descriptor);
                diagnostics.Record(CreateDiagnostic(descriptor.Id, "manifest.invalid", DiagnosticSeverity.Error, "plugin.manifest.invalid"));
            }
        }

        return descriptors;
    }

    private static PluginManifest EmptyManifest(string id) =>
        new(id, id, string.Empty, string.Empty, string.Empty, null, null, null, string.Empty, string.Empty, string.Empty, string.Empty, null, [], [], [], [], new PluginResources(null, null), [], null, null);

    private static PluginDiagnosticEvent CreateDiagnostic(string pluginId, string eventType, DiagnosticSeverity severity, string messageKey) =>
        new(pluginId, eventType, severity, messageKey, DateTimeOffset.UtcNow, new Dictionary<string, string>());
}

public sealed class PluginLoaderService : IPluginLoaderService
{
    private readonly PluginHostOptions options;
    private readonly IPluginManifestValidator validator;
    private readonly IPluginPermissionService permissions;
    private readonly IPluginRegistry registry;
    private readonly IPluginDiagnosticService diagnostics;
    private readonly IServiceProvider services;
    private readonly IPluginStorage storage;
    private readonly IPluginNotificationService notifications;
    private readonly ILoggerFactory loggerFactory;

    public PluginLoaderService(
        PluginHostOptions options,
        IPluginManifestValidator validator,
        IPluginPermissionService permissions,
        IPluginRegistry registry,
        IPluginDiagnosticService diagnostics,
        IServiceProvider services,
        IPluginStorage storage,
        IPluginNotificationService notifications,
        ILoggerFactory? loggerFactory = null)
    {
        this.options = options;
        this.validator = validator;
        this.permissions = permissions;
        this.registry = registry;
        this.diagnostics = diagnostics;
        this.services = services;
        this.storage = storage;
        this.notifications = notifications;
        this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    public async Task<PluginLoadResult> LoadAsync(PluginDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        if (descriptor.State != PluginDiscoveryState.ReadyForLoad)
        {
            return Fail(descriptor.Id, PluginLoadState.Failed, "Somente plugins ReadyForLoad podem ser carregados.");
        }

        var validation = validator.Validate(descriptor.Manifest, descriptor.DirectoryPath);
        if (!validation.IsValid)
        {
            return Fail(descriptor.Id, PluginLoadState.Failed, validation.Errors.ToArray());
        }

        foreach (var permission in descriptor.Manifest.Permissions)
        {
            var decision = await permissions.RequestPermissionAsync(descriptor.Id, permission, cancellationToken);
            if (decision is PermissionDecision.Denied or PermissionDecision.Blocked or PermissionDecision.Unknown)
            {
                diagnostics.Record(CreateDiagnostic(descriptor.Id, "permission.denied", DiagnosticSeverity.Error, "plugin.permission.denied"));
                return Fail(descriptor.Id, PluginLoadState.Failed, $"Permissão negada: {permission}.");
            }

            if (decision == PermissionDecision.RequiresUserApproval && !permissions.HasPermission(descriptor.Id, permission))
            {
                diagnostics.Record(CreateDiagnostic(descriptor.Id, "permission.required", DiagnosticSeverity.Warning, "plugin.permission.required"));
                return Fail(descriptor.Id, PluginLoadState.Failed, $"Permissão exige aprovação explícita: {permission}.");
            }
        }

        var assemblyPath = Path.GetFullPath(Path.Combine(descriptor.DirectoryPath, descriptor.Manifest.EntryAssembly));
        if (!File.Exists(assemblyPath))
        {
            return Fail(descriptor.Id, PluginLoadState.Failed, $"Assembly não encontrado: {descriptor.Manifest.EntryAssembly}.");
        }

        var loadContext = new AssemblyLoadContext($"WiiStudio.Plugin.{descriptor.Id}", isCollectible: true);
        try
        {
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            var pluginType = assembly.GetType(descriptor.Manifest.EntryType, throwOnError: false);
            if (pluginType is null)
            {
                return FailAndUnload(descriptor.Id, loadContext, $"Entry type não encontrado: {descriptor.Manifest.EntryType}.");
            }

            if (!typeof(IWiiStudioPlugin).IsAssignableFrom(pluginType))
            {
                return FailAndUnload(descriptor.Id, loadContext, "Entry type não implementa IWiiStudioPlugin.");
            }

            if (Activator.CreateInstance(pluginType) is not IWiiStudioPlugin instance)
            {
                return FailAndUnload(descriptor.Id, loadContext, "Não foi possível instanciar o plugin.");
            }

            using var initializeTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            initializeTimeout.CancelAfter(options.InitializeTimeout);
            var context = new PluginContext(
                loggerFactory.CreateLogger($"WiiStudio.Plugin.{descriptor.Id}"),
                services,
                storage,
                permissions,
                notifications);

            await instance.InitializeAsync(context, initializeTimeout.Token);
            registry.RegisterRunningPlugin(descriptor, instance, loadContext);
            diagnostics.Record(CreateDiagnostic(descriptor.Id, "loading.loaded", DiagnosticSeverity.Information, "plugin.loading.loaded"));

            return PluginLoadResult.Succeeded(descriptor.Id, PluginLoadState.Running, instance, validation.Warnings);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            loadContext.Unload();
            return Fail(descriptor.Id, PluginLoadState.Failed, ex.Message);
        }
    }

    public async Task UnloadAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (!registry.Remove(pluginId, out var plugin, out var loadContext) || plugin is null)
        {
            return;
        }

        using var shutdownTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        shutdownTimeout.CancelAfter(options.ShutdownTimeout);
        await plugin.ShutdownAsync(shutdownTimeout.Token);
        loadContext?.Unload();
        diagnostics.Record(CreateDiagnostic(pluginId, "loading.unloaded", DiagnosticSeverity.Information, "plugin.loading.unloaded"));
    }

    private PluginLoadResult Fail(string pluginId, PluginLoadState state, params string[] errors)
    {
        diagnostics.Record(CreateDiagnostic(pluginId, "loading.failed", DiagnosticSeverity.Error, "plugin.loading.failed"));
        return PluginLoadResult.Failed(pluginId, state, errors);
    }

    private PluginLoadResult FailAndUnload(string pluginId, AssemblyLoadContext loadContext, params string[] errors)
    {
        loadContext.Unload();
        return Fail(pluginId, PluginLoadState.Failed, errors);
    }

    private static PluginDiagnosticEvent CreateDiagnostic(string pluginId, string eventType, DiagnosticSeverity severity, string messageKey) =>
        new(pluginId, eventType, severity, messageKey, DateTimeOffset.UtcNow, new Dictionary<string, string>());
}

public sealed class PluginRegistry : IPluginRegistry
{
    private readonly Dictionary<string, PluginDescriptor> descriptors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RunningPlugin> running = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<PluginDescriptor> Descriptors => descriptors.Values.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase).ToArray();

    public IReadOnlyList<IWiiStudioPlugin> RunningPlugins => running.Values.Select(item => item.Plugin).ToArray();

    public bool TryGetDescriptor(string pluginId, out PluginDescriptor descriptor) =>
        descriptors.TryGetValue(pluginId, out descriptor!);

    public bool TryGetRunningPlugin(string pluginId, out IWiiStudioPlugin plugin)
    {
        if (running.TryGetValue(pluginId, out var item))
        {
            plugin = item.Plugin;
            return true;
        }

        plugin = null!;
        return false;
    }

    public void RegisterDescriptor(PluginDescriptor descriptor)
    {
        if (descriptors.ContainsKey(descriptor.Id))
        {
            throw new InvalidOperationException($"Plugin duplicado: {descriptor.Id}.");
        }

        descriptors.Add(descriptor.Id, descriptor);
    }

    public void RegisterRunningPlugin(PluginDescriptor descriptor, IWiiStudioPlugin plugin, AssemblyLoadContext loadContext)
    {
        RegisterDescriptorIfMissing(descriptor);
        running[descriptor.Id] = new RunningPlugin(plugin, loadContext);
    }

    public bool Remove(string pluginId, out IWiiStudioPlugin? plugin, out AssemblyLoadContext? loadContext)
    {
        if (!running.Remove(pluginId, out var item))
        {
            plugin = null;
            loadContext = null;
            return false;
        }

        plugin = item.Plugin;
        loadContext = item.LoadContext;
        return true;
    }

    private void RegisterDescriptorIfMissing(PluginDescriptor descriptor)
    {
        if (!descriptors.ContainsKey(descriptor.Id))
        {
            descriptors.Add(descriptor.Id, descriptor);
        }
    }

    private sealed record RunningPlugin(IWiiStudioPlugin Plugin, AssemblyLoadContext LoadContext);
}

public sealed class PluginContext : IPluginContext
{
    public PluginContext(
        ILogger logger,
        IServiceProvider services,
        IPluginStorage storage,
        IPluginPermissionService permissions,
        IPluginNotificationService notifications)
    {
        Logger = logger;
        Services = services;
        Storage = storage;
        Permissions = permissions;
        Notifications = notifications;
    }

    public ILogger Logger { get; }
    public IServiceProvider Services { get; }
    public IPluginStorage Storage { get; }
    public IPluginPermissionService Permissions { get; }
    public IPluginNotificationService Notifications { get; }
}

public sealed class FileSystemPluginStorage : IPluginStorage
{
    private readonly string rootDirectory;

    public FileSystemPluginStorage()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WiiStudio", "plugins"))
    {
    }

    public FileSystemPluginStorage(string rootDirectory)
    {
        this.rootDirectory = rootDirectory;
    }

    public Task<string> GetRootPathAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(rootDirectory, SanitizePluginId(pluginId));
        Directory.CreateDirectory(path);
        return Task.FromResult(path);
    }

    public async Task<Stream> OpenReadAsync(string pluginId, string relativePath, CancellationToken cancellationToken = default)
    {
        var path = await ResolvePathAsync(pluginId, relativePath, cancellationToken);
        return File.OpenRead(path);
    }

    public async Task<Stream> OpenWriteAsync(string pluginId, string relativePath, CancellationToken cancellationToken = default)
    {
        var path = await ResolvePathAsync(pluginId, relativePath, cancellationToken);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? rootDirectory);
        return File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    private async Task<string> ResolvePathAsync(string pluginId, string relativePath, CancellationToken cancellationToken)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException("Plugin storage aceita apenas caminhos relativos.");
        }

        var root = Path.GetFullPath(await GetRootPathAsync(pluginId, cancellationToken));
        var target = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!target.StartsWith(root, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Plugin storage bloqueou path traversal.");
        }

        return target;
    }

    private static string SanitizePluginId(string pluginId)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            pluginId = pluginId.Replace(invalid, '_');
        }

        return pluginId;
    }
}

public sealed class NullPluginNotificationService : IPluginNotificationService
{
    public Task NotifyAsync(PluginNotification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed record PluginManifestDto(
    string? Id,
    string? Name,
    string? Description,
    string? Version,
    string? Author,
    string? Website,
    string? Repository,
    string? License,
    string? EntryAssembly,
    string? EntryType,
    string? PluginSdkVersion,
    string? MinimumWiiStudioVersion,
    string? MaximumWiiStudioVersion,
    IReadOnlyList<string>? SupportedPlatforms,
    IReadOnlyList<string>? Capabilities,
    IReadOnlyList<string>? Permissions,
    IReadOnlyList<PluginDependencyDto>? Dependencies,
    PluginResourcesDto? Resources,
    IReadOnlyList<string>? Tags,
    string? Category,
    string? ReleaseNotes)
{
    public PluginManifest ToManifest() =>
        new(
            Id ?? string.Empty,
            Name ?? string.Empty,
            Description ?? string.Empty,
            Version ?? string.Empty,
            Author ?? string.Empty,
            Website,
            Repository,
            License,
            EntryAssembly ?? string.Empty,
            EntryType ?? string.Empty,
            PluginSdkVersion ?? string.Empty,
            MinimumWiiStudioVersion ?? string.Empty,
            MaximumWiiStudioVersion,
            SupportedPlatforms ?? [],
            (Capabilities ?? []).Select(ParseCapability).ToArray(),
            (Permissions ?? []).Select(ParsePermission).ToArray(),
            (Dependencies ?? []).Select(item => new PluginDependency(item.PluginId ?? string.Empty, item.MinimumVersion ?? string.Empty, item.Optional)).ToArray(),
            new PluginResources(Resources?.Icon, Resources?.Localization),
            Tags ?? [],
            Category,
            ReleaseNotes);

    private static PluginCapability ParseCapability(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "metadata-provider" => PluginCapability.MetadataProvider,
            "asset-provider" => PluginCapability.AssetProvider,
            "conversion-provider" => PluginCapability.ConversionProvider,
            "diagnostics-provider" => PluginCapability.DiagnosticsProvider,
            "ui-extension" => PluginCapability.UiExtension,
            "automation" => PluginCapability.Automation,
            _ => throw new JsonException($"Capacidade desconhecida: {value}.")
        };

    private static PluginPermission ParsePermission(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "file-system-read" => PluginPermission.FileSystemRead,
            "file-system-write" => PluginPermission.FileSystemWrite,
            "network-access" => PluginPermission.NetworkAccess,
            "execute-process" => PluginPermission.ExecuteProcess,
            "access-game-library" => PluginPermission.AccessGameLibrary,
            "modify-game-library" => PluginPermission.ModifyGameLibrary,
            "access-metadata" => PluginPermission.AccessMetadata,
            "modify-metadata" => PluginPermission.ModifyMetadata,
            "access-assets" => PluginPermission.AccessAssets,
            "modify-assets" => PluginPermission.ModifyAssets,
            "access-diagnostics" => PluginPermission.AccessDiagnostics,
            "ui-extension" => PluginPermission.UiExtension,
            "background-tasks" => PluginPermission.BackgroundTasks,
            _ => throw new JsonException($"Permissão desconhecida: {value}.")
        };
}

internal sealed record PluginDependencyDto(string? PluginId, string? MinimumVersion, bool Optional);

internal sealed record PluginResourcesDto(string? Icon, string? Localization);
