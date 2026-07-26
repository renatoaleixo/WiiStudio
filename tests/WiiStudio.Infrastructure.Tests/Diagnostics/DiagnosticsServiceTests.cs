using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;
using WiiStudio.Infrastructure.AppUpdates;
using WiiStudio.Infrastructure.Backups;
using WiiStudio.Infrastructure.Covers;
using WiiStudio.Infrastructure.Diagnostics;
using WiiStudio.Infrastructure.Favorites;
using WiiStudio.Infrastructure.GameTdb;
using WiiStudio.Infrastructure.Logging;
using WiiStudio.Infrastructure.Metadata;
using WiiStudio.Infrastructure.Notifications;
using WiiStudio.Infrastructure.Profiles;
using WiiStudio.Infrastructure.Settings;
using WiiStudio.Infrastructure.Tasks;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Diagnostics;

public sealed class DiagnosticsServiceTests
{
    [Fact]
    public async Task RunAsync_ShouldReportHealthyWiimmsTools()
    {
        using var fixture = TemporaryDiagnosticsFixture.Create();
        var service = fixture.CreateService(
            new FakeWitService(new Version(3, 5)),
            new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"));

        var report = await service.RunAsync(
            new DiagnosticsOptions(IncludeSystem: false, IncludeStorage: false, IncludeConfiguration: false, IncludeDevices: false),
            CancellationToken.None);

        report.IsHealthy.Should().BeTrue();
        report.Checks.Should().Contain(check =>
            check.Name == "wit" &&
            check.Status == DiagnosticsStatus.Healthy &&
            check.Version == "3.5" &&
            check.Category == "Dependencies");
        report.Checks.Should().Contain(check => check.Name == "wwt" && check.Status == DiagnosticsStatus.Healthy);
        report.Checks.Should().Contain(check =>
            check.Name == "cover-provider" &&
            check.Status == DiagnosticsStatus.Healthy &&
            check.Message.Contains("GameTDB") &&
            check.Message.Contains("https://art.gametdb.com/wii"));
        report.Checks.Should().Contain(check =>
            check.Name == "metadata-provider" &&
            check.Status == DiagnosticsStatus.Healthy &&
            check.Message.Contains("https://www.gametdb.com/wiitdb.zip"));
        report.Checks.Should().Contain(check => check.Name == "external-tool:nkit" && check.Status == DiagnosticsStatus.Warning);
        report.Checks.Should().Contain(check => check.Name == "external-tool:dolphin" && check.Status == DiagnosticsStatus.Warning);
    }

    [Fact]
    public async Task RunAsync_ShouldReportUnhealthyWhenWitIsMissing()
    {
        using var fixture = TemporaryDiagnosticsFixture.Create();
        var service = fixture.CreateService(
            new FakeWitService(new WiimmsNotInstalledException("wit")),
            new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"));

        var report = await service.RunAsync(
            new DiagnosticsOptions(IncludeSystem: false, IncludeStorage: false, IncludeConfiguration: false, IncludeDevices: false),
            CancellationToken.None);

        report.IsHealthy.Should().BeFalse();
        report.Checks.Should().Contain(check =>
            check.Name == "wit" &&
            check.Status == DiagnosticsStatus.Unhealthy &&
            check.Severity == DiagnosticsSeverity.Error);
    }

    [Fact]
    public async Task RunAsync_ShouldReportUnhealthyWhenWwtIsMissing()
    {
        using var fixture = TemporaryDiagnosticsFixture.Create();
        var service = fixture.CreateService(
            new FakeWitService(new Version(3, 5)),
            new FakeWiimmsToolLocator(new WiimmsNotInstalledException("wwt")));

        var report = await service.RunAsync(
            new DiagnosticsOptions(IncludeSystem: false, IncludeStorage: false, IncludeConfiguration: false, IncludeDevices: false),
            CancellationToken.None);

        report.IsHealthy.Should().BeFalse();
        report.Checks.Should().Contain(check => check.Name == "wwt" && check.Status == DiagnosticsStatus.Unhealthy);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeAdvancedSystemStorageConfigurationAndDeviceChecks()
    {
        using var fixture = TemporaryDiagnosticsFixture.Create();
        var service = fixture.CreateService(
            new FakeWitService(new Version(3, 5)),
            new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"),
            usbDrives: [CreateUsbDrive()]);

        var report = await service.RunAsync(new DiagnosticsOptions(IncludeDependencies: false), CancellationToken.None);

        report.Checks.Should().Contain(check => check.Category == "System" && check.Name == "operating-system");
        report.Checks.Should().Contain(check => check.Category == "Storage" && check.Name == "settings");
        report.Checks.Should().Contain(check => check.Category == "Configuration" && check.Name == "diagnostics-enabled");
        report.Checks.Should().Contain(check => check.Category == "Devices" && check.Name == "usb-drives" && check.Message.Contains("1 compatible"));
        report.Checks.Should().Contain(check => check.Category == "Privileged Device Access" && check.Name == "privileged-device-access");
        report.Checks.Should().Contain(check => check.Category == "Polkit Policy" && check.Name == "polkit-policy");
        report.Checks.Should().Contain(check => check.Category == "Devices" && check.Name == "usb-device-provider");
    }

    [Fact]
    public async Task ExportAsync_ShouldWriteJsonReport()
    {
        using var fixture = TemporaryDiagnosticsFixture.Create();
        var path = Path.Combine(fixture.RootDirectory, "diagnostics.json");
        var service = fixture.CreateService(
            new FakeWitService(new Version(3, 5)),
            new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"));

        await service.ExportAsync(path, CancellationToken.None);

        var json = await File.ReadAllTextAsync(path);
        json.Should().Contain("\"name\": \"wit\"");
        json.Should().Contain("\"status\": 1");
    }

    [Fact]
    public async Task ExportAsync_ShouldMaskSensitiveMessages()
    {
        using var fixture = TemporaryDiagnosticsFixture.Create();
        var path = Path.Combine(fixture.RootDirectory, "diagnostics.json");
        var service = fixture.CreateService(
            new FakeWitService(new Version(3, 5)),
            new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"));
        var report = new DiagnosticsReport(
            DateTimeOffset.UtcNow,
            [new DiagnosticsCheckResult("auth", DiagnosticsStatus.Warning, "token=abc123", Category: "Configuration")]);

        await service.ExportAsync(report, path, CancellationToken.None);

        var json = await File.ReadAllTextAsync(path);
        json.Should().Contain("token=***");
        json.Should().NotContain("abc123");
    }

    private static UsbDrive CreateUsbDrive()
    {
        return new UsbDrive("USB", "/media/wii", UsbDriveFileSystem.Fat32, 1024, 512);
    }

    private sealed class FakeWitService : IWitService
    {
        private readonly Version? _version;
        private readonly Exception? _exception;

        public FakeWitService(Version version)
        {
            _version = version;
        }

        public FakeWitService(Exception exception)
        {
            _exception = exception;
        }

        public Task<Version> GetVersionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_version!);
        }

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<Game>>([]);
        }

        public Task VerifyAsync(string sourcePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }

        public Task CopyAsync(string sourcePath, string destinationPath, WiiStudio.Domain.Enums.FileFormat targetFormat, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }

    private sealed class FakeWiimmsToolLocator : IWiimmsToolLocator
    {
        private readonly string? _wwtPath;
        private readonly Exception? _exception;

        public FakeWiimmsToolLocator(string wwtPath)
        {
            _wwtPath = wwtPath;
        }

        public FakeWiimmsToolLocator(Exception exception)
        {
            _exception = exception;
        }

        public Task<string> FindWitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult("/opt/wiimms/wit");
        }

        public Task<string> FindWwtAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_wwtPath!);
        }
    }

    private sealed class FakeExternalToolManager : IExternalToolManager
    {
        public Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ExternalToolStatus> statuses =
            [
                new("wiimms-wit", "Wiimms WIT", ExternalToolState.Configured, "/opt/wiimms/wit", "3.5", "test", null, DateTimeOffset.UtcNow),
                new("wiimms-wwt", "Wiimms WWT", ExternalToolState.Configured, "/opt/wiimms/wwt", "3.5", "test", null, DateTimeOffset.UtcNow),
                new("nkit", "NKit Legacy", ExternalToolState.NotInstalled, null, null, null, "externalTools.notInstalled", DateTimeOffset.UtcNow),
                new("dolphin", "Dolphin Emulator", ExternalToolState.NotInstalled, null, null, null, "externalTools.notInstalled", DateTimeOffset.UtcNow)
            ];

            return Task.FromResult(statuses);
        }

        public Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ExternalToolStatus(toolId, toolId, ExternalToolState.NotInstalled, null, null, null, null, DateTimeOffset.UtcNow));
        }

        public Task<ExternalToolInstallResult> InstallAsync(ExternalToolInstallRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExternalToolInstallResult.Failed(request.ToolId, "not-implemented"));
        }

        public Task<ExternalToolStatus> ConfigureManualPathAsync(ExternalToolManualPathRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ExternalToolStatus(request.ToolId, request.ToolId, ExternalToolState.Configured, request.ExecutablePath, null, "manual", null, DateTimeOffset.UtcNow));
        }
    }

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive>? drives = null)
        {
            _drives = drives ?? [];
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_drives);
        }
    }

    private sealed class FakeDevicePermissionService : IDevicePermissionService
    {
        public Task<DevicePermissionStatus> CheckAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new DevicePermissionStatus(
                OSPlatformKind.Linux,
                PrivilegedAccessState.AuthorizationRequired,
                CanRetry: true,
                RequiresUserAction: true,
                "Autorização administrativa pode ser necessária.",
                "Autorize o prompt do sistema quando solicitado.",
                "test"));
        }

        public Task<DevicePermissionStatus> RequestAuthorizationAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default)
        {
            return CheckAsync(request, cancellationToken);
        }
    }

    private sealed class FakePolkitPolicyInstaller : IPolkitPolicyInstaller
    {
        public Task<PolkitPolicyInstallStatus> CheckAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new PolkitPolicyInstallStatus(
                PolkitPolicyInstallState.Installed,
                "/usr/share/polkit-1/actions/com.wiistudio.device.policy",
                "Policy Polkit instalada e válida.",
                "Nenhuma ação necessária."));
        }

        public Task<PolkitPolicyInstallResult> InstallAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new PolkitPolicyInstallResult(
                PolkitPolicyInstallState.Installed,
                Success: true,
                "/usr/share/polkit-1/actions/com.wiistudio.device.policy",
                "Policy instalada com sucesso.",
                "Nenhuma ação necessária."));
        }

        public Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new PolkitPolicyValidationResult(
                IsValid: true,
                PolkitPolicyInstallState.Installed,
                "/usr/share/polkit-1/actions/com.wiistudio.device.policy",
                HasExpectedAction: true,
                HasAuthAdminKeep: true,
                HasExpectedVendor: true,
                []));
        }
    }

    private sealed class FakeUsbDeviceProvider : IUsbDeviceProvider
    {
        public Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<UsbDeviceInfo> devices =
            [
                new(
                    "usb-1",
                    "Wii USB",
                    "/dev/sdc",
                    "/dev/sdc1",
                    IsUsb: true,
                    IsRemovable: true,
                    1024,
                    [new UsbPartitionInfo("usb-1p1", "/dev/sdc1", "/dev/sdc1", "Wbfs", 1024, 512, IsWbfsCandidate: true, IsWritable: true)])
            ];
            return Task.FromResult(devices);
        }
    }

    private sealed class TemporaryDiagnosticsFixture : IDisposable
    {
        private TemporaryDiagnosticsFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public string RootDirectory { get; }

        public static TemporaryDiagnosticsFixture Create()
        {
            return new TemporaryDiagnosticsFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-diagnostics-{Guid.NewGuid():N}"));
        }

        public DiagnosticsService CreateService(
            IWitService witService,
            IWiimmsToolLocator toolLocator,
            IReadOnlyList<UsbDrive>? usbDrives = null)
        {
            return new DiagnosticsService(
                witService,
                toolLocator,
                new FakeExternalToolManager(),
                new SettingsStoreOptions { FilePath = Path.Combine(RootDirectory, "config", "settings.json") },
                new ProfileStoreOptions { RootDirectory = Path.Combine(RootDirectory, "profiles") },
                new FavoriteGameStoreOptions { FilePath = Path.Combine(RootDirectory, "favorites.json") },
                new MetadataCacheOptions { FilePath = Path.Combine(RootDirectory, "metadata", "games.json") },
                new GameTdbMetadataProviderOptions { DatabaseDirectory = Path.Combine(RootDirectory, "metadata", "gametdb") },
                new NotificationStoreOptions { FilePath = Path.Combine(RootDirectory, "notifications", "history.json") },
                new TaskManagerOptions { FilePath = Path.Combine(RootDirectory, "tasks", "tasks.json") },
                new GameTdbUpdateStateStoreOptions { FilePath = Path.Combine(RootDirectory, "metadata", "gametdb-state.json") },
                new GameTdbCoverProviderOptions(),
                new ApplicationUpdateStateStoreOptions { FilePath = Path.Combine(RootDirectory, "updates", "app-update-state.json") },
                new CoverCacheOptions { RootDirectory = Path.Combine(RootDirectory, "assets") },
                new LoggingCenterOptions { FilePath = Path.Combine(RootDirectory, "logs", "wiistudio.jsonl") },
                new BackupServiceOptions { RestorePointDirectory = Path.Combine(RootDirectory, "restore-points") },
                new FakeUsbDriveService(usbDrives),
                new FakeDevicePermissionService(),
                new FakePolkitPolicyInstaller(),
                new FakeUsbDeviceProvider(),
                NullLogger<DiagnosticsService>.Instance);
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }
}
