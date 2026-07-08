using Microsoft.Extensions.Logging;

namespace WiiStudio.Plugin.Abstractions;

public interface IWiiStudioPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }

    Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default);

    Task ShutdownAsync(CancellationToken cancellationToken = default);
}

public interface IPluginContext
{
    ILogger Logger { get; }
    IServiceProvider Services { get; }
    IPluginStorage Storage { get; }
    IPluginPermissionService Permissions { get; }
    IPluginNotificationService Notifications { get; }
}

public interface IPluginStorage
{
    Task<string> GetRootPathAsync(string pluginId, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string pluginId, string relativePath, CancellationToken cancellationToken = default);
    Task<Stream> OpenWriteAsync(string pluginId, string relativePath, CancellationToken cancellationToken = default);
}

public interface IPluginPermissionService
{
    bool HasPermission(string pluginId, PluginPermission permission);

    Task<PermissionDecision> RequestPermissionAsync(
        string pluginId,
        PluginPermission permission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PluginPermission>> GetGrantedPermissionsAsync(
        string pluginId,
        CancellationToken cancellationToken = default);
}

public interface IPluginNotificationService
{
    Task NotifyAsync(PluginNotification notification, CancellationToken cancellationToken = default);
}

public interface IPluginManifest
{
    string Id { get; }
    string Name { get; }
    string Version { get; }
    string EntryAssembly { get; }
    string EntryType { get; }
    string PluginSdkVersion { get; }
    string MinimumWiiStudioVersion { get; }
    IReadOnlyList<PluginCapability> Capabilities { get; }
    IReadOnlyList<PluginPermission> Permissions { get; }
}

public interface IMetadataProviderPlugin : IWiiStudioPlugin
{
    string ProviderId { get; }
    int Priority { get; }

    Task<MetadataProviderResult> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<MetadataProviderResult> GetByGameIdAsync(
        string gameId,
        CancellationToken cancellationToken = default);
}

public interface IAssetProviderPlugin : IWiiStudioPlugin
{
    string ProviderId { get; }

    Task<PluginAssetResult> GetAssetAsync(
        PluginAssetRequest request,
        CancellationToken cancellationToken = default);
}

public interface IConversionProviderPlugin : IWiiStudioPlugin
{
    string ProviderId { get; }
    string DisplayName { get; }
    IReadOnlyList<string> SupportedInputFormats { get; }
    IReadOnlyList<string> SupportedOutputFormats { get; }

    Task<ConversionValidationResult> ValidateAsync(
        ConversionProviderRequest request,
        CancellationToken cancellationToken = default);

    Task<ConversionProviderResult> ConvertAsync(
        ConversionProviderRequest request,
        IProgress<ConversionProgress> progress,
        CancellationToken cancellationToken = default);
}

public interface IDiagnosticsPlugin : IWiiStudioPlugin
{
    Task<IReadOnlyList<PluginDiagnosticResult>> CollectDiagnosticsAsync(
        CancellationToken cancellationToken = default);
}

public interface IUiExtensionPlugin : IWiiStudioPlugin
{
    IReadOnlyList<UiExtension> GetExtensions();
}

public enum PluginPermission
{
    FileSystemRead,
    FileSystemWrite,
    NetworkAccess,
    ExecuteProcess,
    AccessGameLibrary,
    ModifyGameLibrary,
    AccessMetadata,
    ModifyMetadata,
    AccessAssets,
    ModifyAssets,
    AccessDiagnostics,
    UiExtension,
    BackgroundTasks
}

public enum PermissionDecision
{
    Granted,
    Denied,
    RequiresUserApproval,
    Blocked,
    Unknown
}

public enum PluginRiskLevel
{
    Low,
    Medium,
    High,
    Critical,
    Unknown
}

public enum PluginCapability
{
    MetadataProvider,
    AssetProvider,
    ConversionProvider,
    DiagnosticsProvider,
    UiExtension,
    Automation
}

public enum PluginDiscoveryState
{
    Unknown,
    Found,
    InvalidManifest,
    Duplicate,
    Disabled,
    Quarantined,
    Incompatible,
    ReadyForLoad
}

public enum PluginLoadState
{
    Pending,
    Loading,
    Loaded,
    Initializing,
    Running,
    Failed,
    Disabled,
    Unloading,
    Unloaded
}

public enum UiExtensionRegion
{
    DashboardCards,
    GameDetailsTabs,
    SettingsSections,
    DiagnosticsPanels,
    ToolbarActions,
    ContextMenuActions
}

public enum DiagnosticSeverity
{
    Trace,
    Information,
    Warning,
    Error,
    Critical
}

public enum PluginCatalogState
{
    Unknown,
    Available,
    Installed,
    UpdateAvailable,
    Disabled,
    Blocked,
    Incompatible
}

public sealed record PluginManifest(
    string Id,
    string Name,
    string Description,
    string Version,
    string Author,
    string? Website,
    string? Repository,
    string? License,
    string EntryAssembly,
    string EntryType,
    string PluginSdkVersion,
    string MinimumWiiStudioVersion,
    string? MaximumWiiStudioVersion,
    IReadOnlyList<string> SupportedPlatforms,
    IReadOnlyList<PluginCapability> Capabilities,
    IReadOnlyList<PluginPermission> Permissions,
    IReadOnlyList<PluginDependency> Dependencies,
    PluginResources Resources,
    IReadOnlyList<string> Tags,
    string? Category,
    string? ReleaseNotes) : IPluginManifest;

public sealed record PluginDependency(string PluginId, string MinimumVersion, bool Optional);

public sealed record PluginResources(string? Icon, string? Localization);

public sealed record PluginDescriptor(
    string Id,
    string Name,
    string Version,
    string DirectoryPath,
    string ManifestPath,
    PluginManifest Manifest,
    PluginDiscoveryState State,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors);

public sealed record PluginLoadResult(
    string PluginId,
    bool Success,
    PluginLoadState State,
    IWiiStudioPlugin? Instance,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public static PluginLoadResult Failed(string pluginId, PluginLoadState state, params string[] errors) =>
        new(pluginId, false, state, null, errors, []);

    public static PluginLoadResult Succeeded(string pluginId, PluginLoadState state, IWiiStudioPlugin instance, IReadOnlyList<string> warnings) =>
        new(pluginId, true, state, instance, [], warnings);
}

public sealed record PluginValidationResult(
    bool IsValid,
    PluginDiscoveryState State,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

public sealed record PluginNotification(string PluginId, string MessageKey, DiagnosticSeverity Severity);

public sealed record MetadataSearchRequest(
    string? GameId,
    string? Title,
    string? Region,
    string? Platform,
    string? FileHash,
    string? Language);

public sealed record MetadataProviderResult(
    bool Success,
    string ProviderId,
    IReadOnlyList<GameMetadata> Items,
    string? ErrorMessage,
    TimeSpan Duration,
    bool FromCache);

public sealed record GameMetadata(
    string GameId,
    string Title,
    string? Subtitle,
    string? Description,
    string? Developer,
    string? Publisher,
    string? Genre,
    string? Region,
    int? ReleaseYear,
    int? PlayerCount,
    string? Rating,
    IReadOnlyList<string> Languages);

public sealed record PluginAssetRequest(string GameId, string AssetType, string? Region, string? Language);

public sealed record PluginAssetResult(
    bool Success,
    string ProviderId,
    string? AssetPath,
    Stream? Content,
    string? ErrorMessage);

public sealed record ConversionProviderRequest(
    string SourcePath,
    string DestinationPath,
    string InputFormat,
    string OutputFormat,
    bool PreserveOriginal,
    IReadOnlyDictionary<string, string> Options);

public sealed record ConversionValidationResult(bool IsValid, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings);

public sealed record ConversionProviderResult(
    bool Success,
    string? OutputPath,
    string? ErrorMessage,
    TimeSpan Duration,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Logs);

public sealed record ConversionProgress(double? Percentage, string? CurrentOperationKey, string? CurrentFile, bool IsIndeterminate);

public sealed record PluginDiagnosticResult(string Key, DiagnosticSeverity Severity, string Message, IReadOnlyDictionary<string, string> Data);

public abstract record UiExtension(
    string PluginId,
    string Id,
    string TitleKey,
    UiExtensionRegion Region,
    string? IconKey,
    int Order,
    bool IsEnabled);

public sealed record PluginDiagnosticEvent(
    string PluginId,
    string EventType,
    DiagnosticSeverity Severity,
    string MessageKey,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, string> Data);

public sealed record PluginDiagnosticReport(
    string PluginId,
    string PluginName,
    string PluginVersion,
    PluginLoadState State,
    IReadOnlyList<PluginDiagnosticEvent> Events,
    IReadOnlyList<string> Recommendations);

public sealed record PluginCatalogItem(
    string Id,
    string Name,
    string Version,
    string Author,
    string Description,
    string Category,
    string? IconUrl,
    string? DownloadUrl,
    string? RepositoryUrl,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Capabilities,
    PluginCatalogState State);
