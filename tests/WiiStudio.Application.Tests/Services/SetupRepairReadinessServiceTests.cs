using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Application.Tests.Services;

public sealed class SetupRepairReadinessServiceTests
{
    [Fact]
    public async Task DiagnosticsRepairService_ShouldExposePolkitRepairAndRequireConfirmation()
    {
        var service = CreateRepairService();
        var check = new DiagnosticsCheckResult("polkit-policy", DiagnosticsStatus.Unhealthy, "missing");

        var actions = await service.GetAvailableActionsAsync(check, CancellationToken.None);
        var denied = await service.ExecuteAsync(new DiagnosticRepairRequest(DiagnosticsRepairService.InstallPolkitPolicyAction), CancellationToken.None);
        var executed = await service.ExecuteAsync(new DiagnosticRepairRequest(DiagnosticsRepairService.InstallPolkitPolicyAction, UserConfirmed: true), CancellationToken.None);

        actions.Should().ContainSingle(action => action.ActionId == DiagnosticsRepairService.InstallPolkitPolicyAction);
        denied.Success.Should().BeFalse();
        executed.Success.Should().BeTrue();
        executed.RefreshedReport.Should().NotBeNull();
    }

    [Fact]
    public async Task FirstRunSetupService_ShouldPersistCompletion()
    {
        var settings = new FakeSettingsService();
        var service = new FirstRunSetupService(
            settings,
            new FakeExternalToolManager(),
            new FakePolkitPolicyInstaller(),
            new FakeGameTdbUpdateService(),
            new FakeNotificationService());

        var result = await service.CompleteAsync(
            new FirstRunSetupRequest("pt-BR", "Dark", "/library", "/exports", UserConfirmedDownloads: false),
            CancellationToken.None);

        result.Completed.Should().BeTrue();
        settings.Settings.FirstRunCompleted.Should().BeTrue();
        settings.Settings.Theme.Should().Be("Dark");
        settings.Settings.LibraryPath.Should().Be("/library");
    }

    [Fact]
    public async Task WiiUsbReadinessService_ShouldReturnReadyWhenAllChecksPass()
    {
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs);
        var metadata = new FakeGameMetadataRepository();
        metadata.Items["RMCP01"] = new GameMetadataDto("RMCP01", "Mario Kart Wii", Region.Pal);
        var assets = new FakeGameAssetRepository();
        var coverPath = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(coverPath, [1, 2, 3]);
            assets.Items["RMCP01"] = [new AssetInfo(new AssetKey("RMCP01", CoverAssetKind.FrontCover), coverPath, 3, DateTimeOffset.UtcNow, CoverAssetSource.Cache)];
            var service = new WiiUsbReadinessService(
                new FakeUsbDriveService([drive]),
                new FakeGameLibraryScanner([CreateGame("RMCP01")]),
                new FakeExternalToolManager(),
                new FakeDevicePermissionService(PrivilegedAccessState.Authorized),
                metadata,
                assets,
                new FakeTaskManager());

            var result = await service.CheckAsync(drive.Id, CancellationToken.None);

            result.State.Should().Be(ReadinessState.Ready);
            result.Items.Should().Contain(item => item.CheckId == "games" && item.State == ReadinessState.Ready);
        }
        finally
        {
            File.Delete(coverPath);
        }
    }

    [Fact]
    public async Task WiiUsbReadinessService_ShouldFlagMissingWiimmsAsNotReady()
    {
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs);
        var tools = new FakeExternalToolManager { ToolsReady = false };
        var service = new WiiUsbReadinessService(
            new FakeUsbDriveService([drive]),
            new FakeGameLibraryScanner([]),
            tools,
            new FakeDevicePermissionService(PrivilegedAccessState.Authorized),
            new FakeGameMetadataRepository(),
            new FakeGameAssetRepository(),
            new FakeTaskManager());

        var result = await service.CheckAsync(drive.Id, CancellationToken.None);

        result.State.Should().Be(ReadinessState.NotReady);
        result.Items.Should().Contain(item => item.CheckId == "wiimms" && item.RepairActionId == DiagnosticsRepairService.ValidateExternalToolsAction);
    }

    private static DiagnosticsRepairService CreateRepairService()
    {
        return new DiagnosticsRepairService(
            new FakePolkitPolicyInstaller(),
            new FakeExternalToolManager(),
            new FakeGameTdbUpdateService(),
            new FakeDiagnosticsService(),
            new FakeTaskManager(),
            new FakeNotificationService());
    }

    private static UsbDrive CreateDrive(UsbDriveFileSystem fileSystem)
    {
        return new UsbDrive(Guid.NewGuid(), "/dev/sdb1", "/media/wii", "Wii HD", fileSystem, 1024L * 1024L * 1024L, 128, 1024L * 1024L * 1024L - 128, true, true);
    }

    private static GameDto CreateGame(string gameId)
    {
        return new GameDto(gameId, "Mario Kart Wii", Region.Pal, DiscType.Wii, FileFormat.Wbfs, $"/games/{gameId}.wbfs", 1024, DateTimeOffset.UtcNow);
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public AppSettings Settings { get; private set; } = new();

        public Task<AppSettings> GetAsync(CancellationToken cancellationToken) => Task.FromResult(Settings);

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(Settings);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            Settings = settings;
            return Task.CompletedTask;
        }

        public Task ResetAsync(CancellationToken cancellationToken)
        {
            Settings = new AppSettings();
            return Task.CompletedTask;
        }

        public Task ExportAsync(string filePath, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ImportAsync(string filePath, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeExternalToolManager : IExternalToolManager
    {
        public bool ToolsReady { get; init; } = true;

        public Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ExternalToolStatus>>([CreateStatus("wiimms-wit"), CreateStatus("wiimms-wwt")]);
        }

        public Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateStatus(toolId));
        }

        public Task<ExternalToolInstallResult> InstallAsync(ExternalToolInstallRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExternalToolInstallResult.Failed(request.ToolId, "not-implemented"));
        }

        public Task<ExternalToolStatus> ConfigureManualPathAsync(ExternalToolManualPathRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateStatus(request.ToolId) with { ExecutablePath = request.ExecutablePath });
        }

        private ExternalToolStatus CreateStatus(string toolId)
        {
            return new ExternalToolStatus(
                toolId,
                toolId,
                ToolsReady ? ExternalToolState.Configured : ExternalToolState.NotInstalled,
                ToolsReady ? $"/usr/bin/{toolId}" : null,
                ToolsReady ? "1.0" : null,
                "test",
                ToolsReady ? null : "missing",
                DateTimeOffset.UtcNow);
        }
    }

    private sealed class FakePolkitPolicyInstaller : IPolkitPolicyInstaller
    {
        public Task<PolkitPolicyInstallStatus> CheckAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PolkitPolicyInstallStatus(PolkitPolicyInstallState.Installed, "/policy", "ok", null));
        }

        public Task<PolkitPolicyInstallResult> InstallAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PolkitPolicyInstallResult(PolkitPolicyInstallState.Installed, Success: true, "/policy", "ok", null));
        }

        public Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PolkitPolicyValidationResult(true, PolkitPolicyInstallState.Installed, "/policy", true, true, true, []));
        }
    }

    private sealed class FakeGameTdbUpdateService : IGameTdbUpdateService
    {
        public Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new UpdateCheckResult(false, "1", new GameTdbUpdateInfo("1", DateTimeOffset.UtcNow, "test"), DateTimeOffset.UtcNow, "ok"));
        }

        public Task<UpdateResult> UpdateAsync(GameTdbUpdateOptions options, CancellationToken cancellationToken)
        {
            return Task.FromResult(new UpdateResult(GameTdbUpdateStatus.Applied, "1", null, 1, DateTimeOffset.UtcNow, "updated"));
        }

        public Task RollbackAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeDiagnosticsService : IDiagnosticsService
    {
        public Task<DiagnosticsReport> RunAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new DiagnosticsReport(DateTimeOffset.UtcNow, [new DiagnosticsCheckResult("ok", DiagnosticsStatus.Healthy, "ok")]));
        }

        public Task<DiagnosticsReport> RunAsync(DiagnosticsOptions options, CancellationToken cancellationToken) => RunAsync(cancellationToken);

        public Task ExportAsync(string path, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ExportAsync(DiagnosticsReport report, string outputFile, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeTaskManager : ITaskManager
    {
        public Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken) => Task.FromResult(Guid.NewGuid());

        public Task UpdateProgressAsync(Guid taskId, double progress, ManagedTaskStatus status, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task CompleteAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task FailAsync(Guid taskId, string errorMessage, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task PauseAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ResumeAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task CancelAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<TaskInfo>> GetActiveTasksAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TaskInfo>>([]);

        public Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TaskInfo>>([]);
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DismissAsync(Guid notificationId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<NotificationMessage>> GetHistoryAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<NotificationMessage>>([]);

        public Task ClearHistoryAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken) => Task.FromResult(_drives);
    }

    private sealed class FakeGameLibraryScanner : IGameLibraryScanner
    {
        private readonly IReadOnlyList<GameDto> _games;

        public FakeGameLibraryScanner(IReadOnlyList<GameDto> games)
        {
            _games = games;
        }

        public Task<LibraryScanResult> ScanAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            return Task.FromResult(new LibraryScanResult(_games, [], DateTimeOffset.UtcNow));
        }
    }

    private sealed class FakeDevicePermissionService : IDevicePermissionService
    {
        private readonly PrivilegedAccessState _state;

        public FakeDevicePermissionService(PrivilegedAccessState state)
        {
            _state = state;
        }

        public Task<DevicePermissionStatus> CheckAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DevicePermissionStatus(OSPlatformKind.Linux, _state, CanRetry: true, RequiresUserAction: false, "ok", null, "ok"));
        }

        public Task<DevicePermissionStatus> RequestAuthorizationAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default)
        {
            return CheckAsync(request, cancellationToken);
        }
    }

    private sealed class FakeGameMetadataRepository : IGameMetadataRepository
    {
        public Dictionary<string, GameMetadataDto> Items { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken)
        {
            Items.TryGetValue(gameId, out var metadata);
            return Task.FromResult(metadata);
        }

        public Task UpsertAsync(GameMetadataDto metadata, CancellationToken cancellationToken)
        {
            Items[metadata.GameId] = metadata;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string gameId, CancellationToken cancellationToken)
        {
            Items.Remove(gameId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGameAssetRepository : IGameAssetRepository
    {
        public Dictionary<string, IReadOnlyList<AssetInfo>> Items { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task UpsertAsync(AssetInfo asset, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteAsync(AssetKey key, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<AssetInfo>> GetByGameIdAsync(string gameId, CancellationToken cancellationToken)
        {
            Items.TryGetValue(gameId, out var assets);
            return Task.FromResult(assets ?? []);
        }
    }
}
