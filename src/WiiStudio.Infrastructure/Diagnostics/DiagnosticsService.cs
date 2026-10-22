using System.Text.Json;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.AppUpdates;
using WiiStudio.Infrastructure.Backups;
using WiiStudio.Infrastructure.Covers;
using WiiStudio.Infrastructure.Emulators;
using WiiStudio.Infrastructure.Favorites;
using WiiStudio.Infrastructure.GameTdb;
using WiiStudio.Infrastructure.Logging;
using WiiStudio.Infrastructure.Metadata;
using WiiStudio.Infrastructure.Notifications;
using WiiStudio.Infrastructure.Profiles;
using WiiStudio.Infrastructure.Settings;
using WiiStudio.Infrastructure.Tasks;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Diagnostics;

public sealed class DiagnosticsService : IDiagnosticsService
{
    private static readonly Regex SensitiveValueRegex = new(
        @"(?<key>password|token|secret)=[^\s;,\}]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly IWitService _witService;
    private readonly IWiimmsToolLocator _toolLocator;
    private readonly IExternalToolManager _externalToolManager;
    private readonly SettingsStoreOptions _settingsOptions;
    private readonly ProfileStoreOptions _profileOptions;
    private readonly FavoriteGameStoreOptions _favoriteOptions;
    private readonly MetadataCacheOptions _metadataOptions;
    private readonly GameTdbMetadataProviderOptions _gameTdbMetadataOptions;
    private readonly NotificationStoreOptions _notificationOptions;
    private readonly TaskManagerOptions _taskOptions;
    private readonly GameTdbUpdateStateStoreOptions _gameTdbOptions;
    private readonly GameTdbCoverProviderOptions _gameTdbCoverOptions;
    private readonly ApplicationUpdateStateStoreOptions _appUpdateOptions;
    private readonly CoverCacheOptions _coverOptions;
    private readonly LoggingCenterOptions _loggingOptions;
    private readonly BackupServiceOptions _backupOptions;
    private readonly IUsbDriveService _usbDriveService;
    private readonly IDevicePermissionService _devicePermissionService;
    private readonly IPolkitPolicyInstaller _polkitPolicyInstaller;
    private readonly IUsbDeviceProvider _usbDeviceProvider;
    private readonly ILogger<DiagnosticsService> _logger;
    private readonly IEmulatorProfileService? _emulatorProfileService;
    private readonly EmulatorSaveOptions? _emulatorSaveOptions;

    public DiagnosticsService(
        IWitService witService,
        IWiimmsToolLocator toolLocator,
        IExternalToolManager externalToolManager,
        SettingsStoreOptions settingsOptions,
        ProfileStoreOptions profileOptions,
        FavoriteGameStoreOptions favoriteOptions,
        MetadataCacheOptions metadataOptions,
        GameTdbMetadataProviderOptions gameTdbMetadataOptions,
        NotificationStoreOptions notificationOptions,
        TaskManagerOptions taskOptions,
        GameTdbUpdateStateStoreOptions gameTdbOptions,
        GameTdbCoverProviderOptions gameTdbCoverOptions,
        ApplicationUpdateStateStoreOptions appUpdateOptions,
        CoverCacheOptions coverOptions,
        LoggingCenterOptions loggingOptions,
        BackupServiceOptions backupOptions,
        IUsbDriveService usbDriveService,
        IDevicePermissionService devicePermissionService,
        IPolkitPolicyInstaller polkitPolicyInstaller,
        IUsbDeviceProvider usbDeviceProvider,
        ILogger<DiagnosticsService> logger,
        IEmulatorProfileService? emulatorProfileService = null,
        EmulatorSaveOptions? emulatorSaveOptions = null)
    {
        _witService = witService;
        _toolLocator = toolLocator;
        _externalToolManager = externalToolManager;
        _settingsOptions = settingsOptions;
        _profileOptions = profileOptions;
        _favoriteOptions = favoriteOptions;
        _metadataOptions = metadataOptions;
        _gameTdbMetadataOptions = gameTdbMetadataOptions;
        _notificationOptions = notificationOptions;
        _taskOptions = taskOptions;
        _gameTdbOptions = gameTdbOptions;
        _gameTdbCoverOptions = gameTdbCoverOptions;
        _appUpdateOptions = appUpdateOptions;
        _coverOptions = coverOptions;
        _loggingOptions = loggingOptions;
        _backupOptions = backupOptions;
        _usbDriveService = usbDriveService;
        _devicePermissionService = devicePermissionService;
        _polkitPolicyInstaller = polkitPolicyInstaller;
        _usbDeviceProvider = usbDeviceProvider;
        _logger = logger;
        _emulatorProfileService = emulatorProfileService;
        _emulatorSaveOptions = emulatorSaveOptions;
    }

    public async Task<DiagnosticsReport> RunAsync(CancellationToken cancellationToken)
    {
        return await RunAsync(new DiagnosticsOptions(), cancellationToken);
    }

    public async Task<DiagnosticsReport> RunAsync(DiagnosticsOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var checks = new List<DiagnosticsCheckResult>();
        if (options.IncludeSystem)
        {
            checks.AddRange(await RunCheckGroupAsync(CheckSystemAsync, cancellationToken));
        }

        if (options.IncludeDependencies)
        {
            checks.AddRange(await RunCheckGroupAsync(CheckDependenciesAsync, cancellationToken));
        }

        if (options.IncludeStorage)
        {
            checks.AddRange(await RunCheckGroupAsync(CheckStorageAsync, cancellationToken));
        }

        if (options.IncludeConfiguration)
        {
            checks.AddRange(await RunCheckGroupAsync(CheckConfigurationAsync, cancellationToken));
        }

        if (options.IncludeDevices)
        {
            checks.AddRange(await RunCheckGroupAsync(CheckDevicesAsync, cancellationToken));
        }

        var report = new DiagnosticsReport(DateTimeOffset.UtcNow, checks);
        _logger.LogInformation("Diagnostics completed with {CheckCount} check(s)", checks.Count);

        return report;
    }

    public async Task ExportAsync(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();

        var report = await RunAsync(cancellationToken);
        await ExportAsync(report, path, cancellationToken);
    }

    public async Task ExportAsync(DiagnosticsReport report, string outputFile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(outputFile);
        await JsonSerializer.SerializeAsync(stream, Mask(report), JsonOptions, cancellationToken);

        _logger.LogInformation("Diagnostics report exported to {Path}", outputFile);
    }

    private async Task<IReadOnlyList<DiagnosticsCheckResult>> RunCheckGroupAsync(
        Func<CancellationToken, Task<IReadOnlyList<DiagnosticsCheckResult>>> checkGroup,
        CancellationToken cancellationToken)
    {
        try
        {
            return await checkGroup(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Diagnostics check group failed");
            return [new DiagnosticsCheckResult("diagnostics", DiagnosticsStatus.Unhealthy, exception.Message, Category: "Diagnostics", Severity: DiagnosticsSeverity.Error)];
        }
    }

    private Task<IReadOnlyList<DiagnosticsCheckResult>> CheckSystemAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? AppContext.BaseDirectory);
        var checks = new[]
        {
            new DiagnosticsCheckResult("operating-system", DiagnosticsStatus.Healthy, Environment.OSVersion.ToString(), Category: "System"),
            new DiagnosticsCheckResult("architecture", DiagnosticsStatus.Healthy, RuntimeInformation.ProcessArchitecture.ToString(), Category: "System"),
            new DiagnosticsCheckResult("dotnet-runtime", DiagnosticsStatus.Healthy, Environment.Version.ToString(), Environment.Version.ToString(), "System"),
            new DiagnosticsCheckResult("disk-space", drive.AvailableFreeSpace > 1024L * 1024L * 1024L ? DiagnosticsStatus.Healthy : DiagnosticsStatus.Warning, $"{drive.AvailableFreeSpace} bytes available.", Category: "System", Severity: drive.AvailableFreeSpace > 1024L * 1024L * 1024L ? DiagnosticsSeverity.Information : DiagnosticsSeverity.Warning)
        };

        return Task.FromResult<IReadOnlyList<DiagnosticsCheckResult>>(checks);
    }

    private async Task<IReadOnlyList<DiagnosticsCheckResult>> CheckDependenciesAsync(CancellationToken cancellationToken)
    {
        var checks = new List<DiagnosticsCheckResult>
        {
            await CheckWitAsync(cancellationToken),
            await CheckWwtAsync(cancellationToken),
            await CheckExecutableAsync("git", "Git", cancellationToken),
            CheckCoverProvider(),
            CheckMetadataProvider(),
            new("sqlite", DiagnosticsStatus.Healthy, "SQLite support is available through local stores.", Category: "Dependencies")
        };

        checks.AddRange(await CheckExternalToolsAsync(cancellationToken));

        return checks;
    }

    private DiagnosticsCheckResult CheckCoverProvider()
    {
        var status = _gameTdbCoverOptions.AllowAutomaticCoverDownload
            ? DiagnosticsStatus.Healthy
            : DiagnosticsStatus.Warning;
        var severity = status == DiagnosticsStatus.Healthy
            ? DiagnosticsSeverity.Information
            : DiagnosticsSeverity.Warning;
        var fallbackLanguages = string.Join(", ", _gameTdbCoverOptions.FallbackLanguages);
        var message = _gameTdbCoverOptions.AllowAutomaticCoverDownload
            ? $"Cover Provider: GameTDB; Base URL: {_gameTdbCoverOptions.BaseUrl}; Preferred language: {_gameTdbCoverOptions.PreferredLanguage}; Fallback languages: {fallbackLanguages}."
            : "Cover Provider: GameTDB is configured but automatic cover download is disabled.";

        return new DiagnosticsCheckResult("cover-provider", status, message, "GameTDB", "Dependencies", severity);
    }

    private DiagnosticsCheckResult CheckMetadataProvider()
    {
        var databasePath = Path.Combine(_gameTdbMetadataOptions.DatabaseDirectory, "wiitdb.xml");
        var hasLocalDatabase = File.Exists(databasePath);
        var status = _gameTdbMetadataOptions.AllowAutomaticMetadataDownload || hasLocalDatabase
            ? DiagnosticsStatus.Healthy
            : DiagnosticsStatus.Warning;
        var severity = status == DiagnosticsStatus.Healthy
            ? DiagnosticsSeverity.Information
            : DiagnosticsSeverity.Warning;
        var fallbackLanguages = string.Join(", ", _gameTdbMetadataOptions.FallbackLanguages);
        var message = $"Metadata Provider: GameTDB; Base URL: {_gameTdbMetadataOptions.DatabaseUrl}; Preferred language: {_gameTdbMetadataOptions.PreferredLanguage}; Fallback languages: {fallbackLanguages}; Local database: {databasePath}; Provider available: {(_gameTdbMetadataOptions.AllowAutomaticMetadataDownload || hasLocalDatabase).ToString().ToLowerInvariant()}.";

        return new DiagnosticsCheckResult("metadata-provider", status, message, "GameTDB", "Dependencies", severity);
    }

    private async Task<IReadOnlyList<DiagnosticsCheckResult>> CheckExternalToolsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _externalToolManager.CheckAllAsync(cancellationToken);
            return statuses.Select(MapExternalToolStatus).ToArray();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return [new DiagnosticsCheckResult("external-tools", DiagnosticsStatus.Warning, exception.Message, Category: "Dependencies", Severity: DiagnosticsSeverity.Warning)];
        }
    }

    private static DiagnosticsCheckResult MapExternalToolStatus(ExternalToolStatus status)
    {
        var isRequired = status.ToolId is "wiimms-wit" or "wiimms-wwt";
        var healthy = status.State is ExternalToolState.Installed or ExternalToolState.Configured;
        var statusValue = healthy
            ? DiagnosticsStatus.Healthy
            : isRequired
                ? DiagnosticsStatus.Unhealthy
                : DiagnosticsStatus.Warning;
        var severity = statusValue == DiagnosticsStatus.Unhealthy
            ? DiagnosticsSeverity.Error
            : statusValue == DiagnosticsStatus.Warning
                ? DiagnosticsSeverity.Warning
                : DiagnosticsSeverity.Information;
        var message = healthy
            ? $"{status.DisplayName} is available."
            : $"{status.DisplayName} is {status.State}.";

        return new DiagnosticsCheckResult(
            $"external-tool:{status.ToolId}",
            statusValue,
            message,
            status.Version ?? status.ExecutablePath,
            "Dependencies",
            severity);
    }

    private async Task<IReadOnlyList<DiagnosticsCheckResult>> CheckStorageAsync(CancellationToken cancellationToken)
    {
        var paths = new[]
        {
            ("settings", _settingsOptions.FilePath),
            ("profiles", _profileOptions.RootDirectory),
            ("favorites", _favoriteOptions.FilePath),
            ("metadata", _metadataOptions.FilePath),
            ("notifications", _notificationOptions.FilePath),
            ("tasks", _taskOptions.FilePath),
            ("gametdb-state", _gameTdbOptions.FilePath),
            ("app-update-state", _appUpdateOptions.FilePath),
            ("cover-cache", _coverOptions.RootDirectory),
            ("logs", _loggingOptions.FilePath),
            ("restore-points", _backupOptions.RestorePointDirectory)
        };
        var checks = new List<DiagnosticsCheckResult>();
        foreach (var (name, path) in paths)
        {
            checks.Add(await CheckPathWritableAsync(name, path, cancellationToken));
        }

        if (_emulatorSaveOptions is not null)
        {
            checks.Add(await CheckPathWritableAsync("emulator-save-backups", _emulatorSaveOptions.BackupDirectory, cancellationToken));
        }

        return checks;
    }

    private async Task<IReadOnlyList<DiagnosticsCheckResult>> CheckConfigurationAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var checks = new List<DiagnosticsCheckResult>
        {
            CheckPathExists("settings-file", _settingsOptions.FilePath, isRequired: false),
            CheckPathExists("profiles-root", _profileOptions.RootDirectory, isRequired: false),
            CheckPathExists("logs-file", _loggingOptions.FilePath, isRequired: false),
            new DiagnosticsCheckResult("diagnostics-enabled", DiagnosticsStatus.Healthy, "Diagnostics are enabled in the application model.", Category: "Configuration")
        };

        if (_emulatorProfileService is not null)
        {
            var profiles = await _emulatorProfileService.GetProfilesAsync(cancellationToken);
            var defaultCount = profiles.Count(profile => profile.IsDefault && string.Equals(profile.EmulatorId, "dolphin", StringComparison.OrdinalIgnoreCase));
            var healthy = defaultCount == 1;
            checks.Add(new DiagnosticsCheckResult(
                "emulator-profiles",
                healthy ? DiagnosticsStatus.Healthy : DiagnosticsStatus.Unhealthy,
                healthy
                    ? $"{profiles.Count} perfil(is) de emulação; perfil padrão válido."
                    : "Os perfis de emulação não possuem exatamente um perfil padrão do Dolphin.",
                Category: "Configuration",
                Severity: healthy ? DiagnosticsSeverity.Information : DiagnosticsSeverity.Error));
        }

        return checks;
    }

    private async Task<IReadOnlyList<DiagnosticsCheckResult>> CheckDevicesAsync(CancellationToken cancellationToken)
    {
        var checks = new List<DiagnosticsCheckResult>();
        try
        {
            var drives = await _usbDriveService.DetectAsync(cancellationToken);
            checks.Add(new DiagnosticsCheckResult("usb-drives", DiagnosticsStatus.Healthy, $"{drives.Count} compatible USB drive(s) detected.", Category: "Devices"));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            checks.Add(new DiagnosticsCheckResult("usb-drives", DiagnosticsStatus.Warning, exception.Message, Category: "Devices", Severity: DiagnosticsSeverity.Warning));
        }

        checks.Add(await CheckPrivilegedDeviceAccessAsync(cancellationToken));
        checks.Add(await CheckPolkitPolicyAsync(cancellationToken));
        checks.Add(await CheckUsbDeviceProviderAsync(cancellationToken));
        return checks;
    }

    private async Task<DiagnosticsCheckResult> CheckPolkitPolicyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var status = await _polkitPolicyInstaller.CheckAsync(cancellationToken);
            var diagnosticStatus = status.State switch
            {
                PolkitPolicyInstallState.Installed or PolkitPolicyInstallState.NotLinux => DiagnosticsStatus.Healthy,
                PolkitPolicyInstallState.Invalid or PolkitPolicyInstallState.ValidationFailed or PolkitPolicyInstallState.InstallFailed => DiagnosticsStatus.Unhealthy,
                _ => DiagnosticsStatus.Warning
            };
            var severity = diagnosticStatus switch
            {
                DiagnosticsStatus.Healthy => DiagnosticsSeverity.Information,
                DiagnosticsStatus.Unhealthy => DiagnosticsSeverity.Error,
                _ => DiagnosticsSeverity.Warning
            };
            var message = $"{status.MessageKey ?? status.State.ToString()} {status.RecommendedActionKey ?? string.Empty}".Trim();
            return new DiagnosticsCheckResult(
                "polkit-policy",
                diagnosticStatus,
                message,
                status.PolicyPath,
                "Polkit Policy",
                severity);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new DiagnosticsCheckResult(
                "polkit-policy",
                DiagnosticsStatus.Warning,
                exception.Message,
                Category: "Polkit Policy",
                Severity: DiagnosticsSeverity.Warning);
        }
    }

    private async Task<DiagnosticsCheckResult> CheckUsbDeviceProviderAsync(CancellationToken cancellationToken)
    {
        try
        {
            var devices = await _usbDeviceProvider.GetDevicesAsync(cancellationToken);
            var partitions = devices.SelectMany(device => device.Partitions).ToArray();
            var wbfsCandidates = partitions.Count(partition => partition.IsWbfsCandidate);
            var permissionIssues = devices.Count(device => device.PermissionStatus?.State is
                PrivilegedAccessState.AuthorizationRequired
                or PrivilegedAccessState.PolicyMissing
                or PrivilegedAccessState.PermissionDenied
                or PrivilegedAccessState.HelperMissing);
            var status = permissionIssues == 0 ? DiagnosticsStatus.Healthy : DiagnosticsStatus.Warning;
            return new DiagnosticsCheckResult(
                "usb-device-provider",
                status,
                $"Devices found: {devices.Count}; partitions: {partitions.Length}; WBFS candidates: {wbfsCandidates}; permission issues: {permissionIssues}.",
                Category: "Devices",
                Severity: status == DiagnosticsStatus.Healthy ? DiagnosticsSeverity.Information : DiagnosticsSeverity.Warning);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new DiagnosticsCheckResult(
                "usb-device-provider",
                DiagnosticsStatus.Warning,
                exception.Message,
                Category: "Devices",
                Severity: DiagnosticsSeverity.Warning);
        }
    }

    private async Task<DiagnosticsCheckResult> CheckPrivilegedDeviceAccessAsync(CancellationToken cancellationToken)
    {
        try
        {
            var wwtPath = await _toolLocator.FindWwtAsync(cancellationToken);
            var status = await _devicePermissionService.CheckAsync(
                new DevicePermissionRequest(
                    "wwt",
                    wwtPath,
                    DevicePath: null,
                    PrivilegedDeviceOperation.ListWbfsGames),
                cancellationToken);
            var diagnosticStatus = status.State switch
            {
                PrivilegedAccessState.Authorized or PrivilegedAccessState.NotRequired => DiagnosticsStatus.Healthy,
                PrivilegedAccessState.Failed => DiagnosticsStatus.Unhealthy,
                _ => DiagnosticsStatus.Warning
            };
            var severity = diagnosticStatus switch
            {
                DiagnosticsStatus.Healthy => DiagnosticsSeverity.Information,
                DiagnosticsStatus.Unhealthy => DiagnosticsSeverity.Error,
                _ => DiagnosticsSeverity.Warning
            };
            var message = $"{status.Platform}: {status.MessageKey ?? status.State.ToString()}. {status.RecommendedActionKey ?? string.Empty}".Trim();

            return new DiagnosticsCheckResult(
                "privileged-device-access",
                diagnosticStatus,
                message,
                status.State.ToString(),
                "Privileged Device Access",
                severity);
        }
        catch (WiimmsNotInstalledException exception)
        {
            return new DiagnosticsCheckResult(
                "privileged-device-access",
                DiagnosticsStatus.Warning,
                $"wwt não está disponível para validar acesso privilegiado: {exception.Message}",
                Category: "Privileged Device Access",
                Severity: DiagnosticsSeverity.Warning);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new DiagnosticsCheckResult(
                "privileged-device-access",
                DiagnosticsStatus.Warning,
                exception.Message,
                Category: "Privileged Device Access",
                Severity: DiagnosticsSeverity.Warning);
        }
    }

    private async Task<DiagnosticsCheckResult> CheckWitAsync(CancellationToken cancellationToken)
    {
        try
        {
            var version = await _witService.GetVersionAsync(cancellationToken);

            return new DiagnosticsCheckResult("wit", DiagnosticsStatus.Healthy, "wit is available.", version.ToString(), "Dependencies");
        }
        catch (WiimmsNotInstalledException exception)
        {
            return Unhealthy(exception.ToolName, "wit was not found.", exception);
        }
        catch (WiimmsVersionParseException exception)
        {
            return Unhealthy("wit", "wit version could not be parsed.", exception);
        }
        catch (WiimmsCommandException exception)
        {
            return Unhealthy("wit", "wit version command failed.", exception);
        }
    }

    private async Task<DiagnosticsCheckResult> CheckWwtAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _toolLocator.FindWwtAsync(cancellationToken);

            return new DiagnosticsCheckResult("wwt", DiagnosticsStatus.Healthy, "wwt is available.");
        }
        catch (WiimmsNotInstalledException exception)
        {
            return Unhealthy(exception.ToolName, "wwt was not found.", exception);
        }
    }

    private DiagnosticsCheckResult Unhealthy(string name, string message, Exception exception)
    {
        _logger.LogWarning(exception, "Diagnostics check {CheckName} failed", name);

        return new DiagnosticsCheckResult(name, DiagnosticsStatus.Unhealthy, message, Category: "Dependencies", Severity: DiagnosticsSeverity.Error);
    }

    private async Task<DiagnosticsCheckResult> CheckExecutableAsync(string executableName, string displayName, CancellationToken cancellationToken)
    {
        try
        {
            var pathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var executable = pathVariable
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(directory => Path.Combine(directory, executableName))
                .FirstOrDefault(File.Exists);

            return executable is null
                ? new DiagnosticsCheckResult(executableName, DiagnosticsStatus.Warning, $"{displayName} was not found in PATH.", Category: "Dependencies", Severity: DiagnosticsSeverity.Warning)
                : new DiagnosticsCheckResult(executableName, DiagnosticsStatus.Healthy, $"{displayName} is available.", Category: "Dependencies");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new DiagnosticsCheckResult(executableName, DiagnosticsStatus.Warning, exception.Message, Category: "Dependencies", Severity: DiagnosticsSeverity.Warning);
        }
        finally
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private async Task<DiagnosticsCheckResult> CheckPathWritableAsync(string name, string path, CancellationToken cancellationToken)
    {
        try
        {
            var directory = Path.HasExtension(path) ? Path.GetDirectoryName(path) : path;
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = AppContext.BaseDirectory;
            }

            Directory.CreateDirectory(directory);
            var probePath = Path.Combine(directory, $".wiistudio-diagnostics-{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(probePath, "ok", cancellationToken);
            File.Delete(probePath);

            return new DiagnosticsCheckResult(name, DiagnosticsStatus.Healthy, "Path is writable.", Category: "Storage");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new DiagnosticsCheckResult(name, DiagnosticsStatus.Unhealthy, exception.Message, Category: "Storage", Severity: DiagnosticsSeverity.Error);
        }
    }

    private static DiagnosticsCheckResult CheckPathExists(string name, string path, bool isRequired)
    {
        var exists = Path.HasExtension(path) ? File.Exists(path) : Directory.Exists(path);
        if (exists)
        {
            return new DiagnosticsCheckResult(name, DiagnosticsStatus.Healthy, "Path exists.", Category: "Configuration");
        }

        return isRequired
            ? new DiagnosticsCheckResult(name, DiagnosticsStatus.Unhealthy, "Required path does not exist.", Category: "Configuration", Severity: DiagnosticsSeverity.Error)
            : new DiagnosticsCheckResult(name, DiagnosticsStatus.Warning, "Path does not exist yet.", Category: "Configuration", Severity: DiagnosticsSeverity.Warning);
    }

    private static DiagnosticsReport Mask(DiagnosticsReport report)
    {
        return report with
        {
            Checks = report.Checks
                .Select(check => check with { Message = MaskSensitive(check.Message) })
                .ToArray()
        };
    }

    private static string MaskSensitive(string value)
    {
        return SensitiveValueRegex.Replace(value, match => $"{match.Groups["key"].Value}=***");
    }
}
