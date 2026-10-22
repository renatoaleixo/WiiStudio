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
        var history = new FakeDiagnosticsRepairHistoryStore();
        var service = CreateRepairService(history);
        var check = new DiagnosticsCheckResult("polkit-policy", DiagnosticsStatus.Unhealthy, "missing");

        var actions = await service.GetAvailableActionsAsync(check, CancellationToken.None);
        var denied = await service.ExecuteAsync(new DiagnosticRepairRequest(DiagnosticsRepairService.InstallPolkitPolicyAction), CancellationToken.None);
        var executed = await service.ExecuteAsync(new DiagnosticRepairRequest(DiagnosticsRepairService.InstallPolkitPolicyAction, UserConfirmed: true), CancellationToken.None);
        var entries = await service.GetHistoryAsync(cancellationToken: CancellationToken.None);

        actions.Should().ContainSingle(action => action.ActionId == DiagnosticsRepairService.InstallPolkitPolicyAction);
        denied.Success.Should().BeFalse();
        executed.Success.Should().BeTrue();
        executed.RefreshedReport.Should().NotBeNull();
        entries.Should().ContainSingle(entry => entry.ActionId == DiagnosticsRepairService.InstallPolkitPolicyAction && entry.Success);
    }

    [Fact]
    public async Task DiagnosticsRepairService_ShouldHideRepairForHealthyChecks()
    {
        var service = CreateRepairService();
        var check = new DiagnosticsCheckResult("polkit-policy", DiagnosticsStatus.Healthy, "ok");

        var actions = await service.GetAvailableActionsAsync(check, CancellationToken.None);

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task DiagnosticsRepairService_ShouldOfferLocalNKitInstallation()
    {
        var toolManager = new FakeExternalToolManager { NKitInstallSucceeds = true };
        var service = CreateRepairService(externalToolManager: toolManager);
        var check = new DiagnosticsCheckResult("external-tool:nkit", DiagnosticsStatus.Warning, "missing");

        var actions = await service.GetAvailableActionsAsync(check, CancellationToken.None);
        var result = await service.ExecuteAsync(
            new DiagnosticRepairRequest(DiagnosticsRepairService.InstallLocalNKitAction, UserConfirmed: true),
            CancellationToken.None);

        actions.First().ActionId.Should().Be(DiagnosticsRepairService.InstallLocalNKitAction);
        actions.First().RequiresConfirmation.Should().BeTrue();
        result.Success.Should().BeTrue();
        toolManager.NKitInstallRequested.Should().BeTrue();
    }

    [Fact]
    public async Task DiagnosticsRepairService_ShouldBlockUnknownAction()
    {
        var service = CreateRepairService();

        var act = () => service.ExecuteAsync(new DiagnosticRepairRequest("repair.unknown", UserConfirmed: true), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*desconhecida*");
    }

    [Fact]
    public async Task FirstRunSetupService_ShouldPersistCompletion()
    {
        var root = Path.Combine(Path.GetTempPath(), $"wiistudio-first-run-{Guid.NewGuid():N}");
        var library = Path.Combine(root, "library");
        var exports = Path.Combine(root, "exports");
        Directory.CreateDirectory(library);
        Directory.CreateDirectory(exports);
        var settings = new FakeSettingsService();
        var service = new FirstRunSetupService(
            settings,
            new FakeExternalToolManager(),
            new FakePolkitPolicyInstaller(),
            new FakeGameTdbUpdateService(),
            new FakeNotificationService());

        try
        {
            var result = await service.CompleteAsync(
                new FirstRunSetupRequest("pt-BR", "Dark", library, exports, UserConfirmedDownloads: false),
                CancellationToken.None);

            result.Completed.Should().BeTrue();
            result.Summary.Should().Contain(item => item.StartsWith("Ferramentas:", StringComparison.Ordinal));
            settings.Settings.FirstRunCompleted.Should().BeTrue();
            settings.Settings.Theme.Should().Be("Dark");
            settings.Settings.LibraryPath.Should().Be(library);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task FirstRunSetupService_ShouldSkipWithoutRunningChecks()
    {
        var gameTdb = new FakeGameTdbUpdateService();
        var service = new FirstRunSetupService(
            new FakeSettingsService(),
            new FakeExternalToolManager { ToolsReady = false },
            new FakePolkitPolicyInstaller(),
            gameTdb,
            new FakeNotificationService());

        var result = await service.CompleteAsync(
            new FirstRunSetupRequest("pt-BR", "System", null, null, UserConfirmedDownloads: false, Skip: true),
            CancellationToken.None);

        result.Skipped.Should().BeTrue();
        gameTdb.CheckCount.Should().Be(0);
        gameTdb.UpdateCount.Should().Be(0);
    }

    [Fact]
    public async Task FirstRunSetupService_ShouldRejectInvalidFolder()
    {
        var service = new FirstRunSetupService(
            new FakeSettingsService(),
            new FakeExternalToolManager(),
            new FakePolkitPolicyInstaller(),
            new FakeGameTdbUpdateService(),
            new FakeNotificationService());

        var act = () => service.CompleteAsync(
            new FirstRunSetupRequest("pt-BR", "System", "/missing/wiistudio-library", null, UserConfirmedDownloads: false),
            CancellationToken.None);

        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage("*não é válida*");
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

    [Fact]
    public async Task WiiUsbReadinessService_ShouldWarnAboutDuplicateGameIds()
    {
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs);
        var service = new WiiUsbReadinessService(
            new FakeUsbDriveService([drive]),
            new FakeGameLibraryScanner([CreateGame("RMCP01"), CreateGame("RMCP01")]),
            new FakeExternalToolManager(),
            new FakeDevicePermissionService(PrivilegedAccessState.Authorized),
            new FakeGameMetadataRepository(),
            new FakeGameAssetRepository(),
            new FakeTaskManager());

        var result = await service.CheckAsync(drive.Id, CancellationToken.None);

        result.State.Should().Be(ReadinessState.Warning);
        result.Items.Should().Contain(item => item.CheckId == "game-ids" && item.State == ReadinessState.Warning);
    }

    [Fact]
    public async Task WiiUsbReadinessService_ShouldKeepChecklistWhenGameScanFails()
    {
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs);
        var service = new WiiUsbReadinessService(
            new FakeUsbDriveService([drive]),
            new FakeGameLibraryScanner([], ThrowOnScan: true),
            new FakeExternalToolManager(),
            new FakeDevicePermissionService(PrivilegedAccessState.Authorized),
            new FakeGameMetadataRepository(),
            new FakeGameAssetRepository(),
            new FakeTaskManager());

        var result = await service.CheckAsync(drive.Id, CancellationToken.None);

        result.State.Should().Be(ReadinessState.Warning);
        result.Items.Should().Contain(item => item.CheckId == "games" && item.RepairActionId == DiagnosticsRepairService.ValidateExternalToolsAction);
        result.Items.Should().Contain(item => item.CheckId == "filesystem");
    }

    [Fact]
    public async Task WiiUsbReadinessService_ShouldExportTextReport()
    {
        var service = new WiiUsbReadinessService(
            new FakeUsbDriveService([]),
            new FakeGameLibraryScanner([]),
            new FakeExternalToolManager(),
            new FakeDevicePermissionService(PrivilegedAccessState.Authorized),
            new FakeGameMetadataRepository(),
            new FakeGameAssetRepository(),
            new FakeTaskManager());
        var report = new WiiUsbReadinessResult(
            "device-1",
            ReadinessState.Warning,
            [new WiiUsbReadinessCheckItem("wiimms", "Wiimms funcionando", ReadinessState.Warning, "missing", "Configure Wiimms.", DiagnosticsRepairService.ValidateExternalToolsAction)],
            DateTimeOffset.UtcNow);
        var path = Path.Combine(Path.GetTempPath(), $"wiistudio-readiness-{Guid.NewGuid():N}.txt");
        try
        {
            await service.ExportAsync(report, path, CancellationToken.None);

            var text = await File.ReadAllTextAsync(path);
            text.Should().Contain("Relatorio de Prontidao Wii USB");
            text.Should().Contain("Wiimms funcionando");
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static DiagnosticsRepairService CreateRepairService(
        FakeDiagnosticsRepairHistoryStore? historyStore = null,
        FakeExternalToolManager? externalToolManager = null)
    {
        return new DiagnosticsRepairService(
            new FakePolkitPolicyInstaller(),
            externalToolManager ?? new FakeExternalToolManager(),
            new FakeGameTdbUpdateService(),
            new FakeDiagnosticsService(),
            new FakeTaskManager(),
            new FakeNotificationService(),
            historyStore ?? new FakeDiagnosticsRepairHistoryStore());
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

        public bool NKitInstallSucceeds { get; init; }

        public bool NKitInstallRequested { get; private set; }

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
            if (request.ToolId == "nkit" && NKitInstallSucceeds)
            {
                NKitInstallRequested = true;
                return Task.FromResult(new ExternalToolInstallResult(
                    "nkit",
                    true,
                    ExternalToolState.Installed,
                    "/tools/nkit/RecoverToISO.exe",
                    "1.4",
                    null,
                    []));
            }

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
        public int CheckCount { get; private set; }

        public int UpdateCount { get; private set; }

        public Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken)
        {
            CheckCount++;
            return Task.FromResult(new UpdateCheckResult(false, "1", new GameTdbUpdateInfo("1", DateTimeOffset.UtcNow, "test"), DateTimeOffset.UtcNow, "ok"));
        }

        public Task<UpdateResult> UpdateAsync(GameTdbUpdateOptions options, CancellationToken cancellationToken)
        {
            UpdateCount++;
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

    private sealed class FakeDiagnosticsRepairHistoryStore : IDiagnosticsRepairHistoryStore
    {
        private readonly List<DiagnosticRepairHistoryEntry> _entries = [];

        public Task AppendAsync(DiagnosticRepairHistoryEntry entry, CancellationToken cancellationToken = default)
        {
            _entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<DiagnosticRepairHistoryEntry>> GetRecentAsync(
            int limit,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<DiagnosticRepairHistoryEntry>>(
                _entries
                    .OrderByDescending(entry => entry.FinishedAt)
                    .Take(Math.Max(1, limit))
                    .ToArray());
        }
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
        private readonly bool _throwOnScan;

        public FakeGameLibraryScanner(IReadOnlyList<GameDto> games, bool ThrowOnScan = false)
        {
            _games = games;
            _throwOnScan = ThrowOnScan;
        }

        public Task<LibraryScanResult> ScanAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            if (_throwOnScan)
            {
                throw new InvalidOperationException("scan failed");
            }

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
