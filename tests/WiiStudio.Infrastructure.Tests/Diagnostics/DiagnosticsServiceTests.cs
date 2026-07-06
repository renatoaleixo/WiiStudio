using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Application.DTOs;
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
                new SettingsStoreOptions { FilePath = Path.Combine(RootDirectory, "config", "settings.json") },
                new ProfileStoreOptions { RootDirectory = Path.Combine(RootDirectory, "profiles") },
                new FavoriteGameStoreOptions { FilePath = Path.Combine(RootDirectory, "favorites.json") },
                new MetadataCacheOptions { FilePath = Path.Combine(RootDirectory, "metadata", "games.json") },
                new NotificationStoreOptions { FilePath = Path.Combine(RootDirectory, "notifications", "history.json") },
                new TaskManagerOptions { FilePath = Path.Combine(RootDirectory, "tasks", "tasks.json") },
                new GameTdbUpdateStateStoreOptions { FilePath = Path.Combine(RootDirectory, "metadata", "gametdb-state.json") },
                new ApplicationUpdateStateStoreOptions { FilePath = Path.Combine(RootDirectory, "updates", "app-update-state.json") },
                new CoverCacheOptions { RootDirectory = Path.Combine(RootDirectory, "assets") },
                new LoggingCenterOptions { FilePath = Path.Combine(RootDirectory, "logs", "wiistudio.jsonl") },
                new BackupServiceOptions { RestorePointDirectory = Path.Combine(RootDirectory, "restore-points") },
                new FakeUsbDriveService(usbDrives),
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
