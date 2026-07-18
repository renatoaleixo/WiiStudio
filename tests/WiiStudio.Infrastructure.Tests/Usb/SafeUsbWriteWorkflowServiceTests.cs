using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Usb;

namespace WiiStudio.Infrastructure.Tests.Usb;

public sealed class SafeUsbWriteWorkflowServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"wiistudio-safe-write-{Guid.NewGuid():N}");

    [Fact]
    public async Task CreatePreviewAsync_ShouldReturnReadyPreviewForWbfsDrive()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs, freeSpace: 512L * 1024L * 1024L);
        var service = CreateService([drive]);

        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        preview.IsBlocked.Should().BeFalse();
        preview.TargetDevicePath.Should().Be("/dev/sdb1");
        preview.TargetFileSystem.Should().Be(UsbDriveFileSystem.Wbfs);
        preview.UserConfirmationRequired.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockInsufficientSpace()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024 * 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs, freeSpace: 128);
        var service = CreateService([drive]);

        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        preview.IsBlocked.Should().BeTrue();
        preview.BlockReasonKey.Should().Be("usbWrite.insufficientSpace");
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockUnknownFileSystem()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Unknown, freeSpace: 512L * 1024L * 1024L);
        var service = CreateService([drive]);

        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        preview.IsBlocked.Should().BeTrue();
        preview.BlockReasonKey.Should().Be("usbWrite.filesystemUnsupported");
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockSystemDiskTarget()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = new UsbDrive(
            Guid.NewGuid(),
            "/dev/sda1",
            "/",
            "System",
            UsbDriveFileSystem.Fat32,
            1024L * 1024L * 1024L,
            128,
            1024L * 1024L * 1024L - 128,
            true,
            true);
        var service = CreateService([drive]);

        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        preview.IsBlocked.Should().BeTrue();
        preview.BlockReasonKey.Should().Be("usbWrite.systemDiskBlocked");
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockDuplicateGameId()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Fat32, freeSpace: 512L * 1024L * 1024L);
        var wwtService = new FakeWwtService([CreateGame("RMCP01")]);
        var service = CreateService([drive], wwtService: wwtService);

        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        preview.IsBlocked.Should().BeTrue();
        preview.BlockReasonKey.Should().Be("usbWrite.duplicateGame");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRequireUserConfirmation()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs, freeSpace: 512L * 1024L * 1024L);
        var wwtService = new FakeWwtService();
        var service = CreateService([drive], wwtService: wwtService);
        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        var result = await service.ExecuteAsync(new UsbWriteExecutionRequest(preview, UserConfirmed: false), null, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessageKey.Should().Be("usbWrite.confirmationRequired");
        wwtService.AddedSources.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBlockWhenPreviewTargetChanged()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs, freeSpace: 512L * 1024L * 1024L);
        var wwtService = new FakeWwtService();
        var service = CreateService([drive], wwtService: wwtService);
        var preview = (await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None))
            with
            {
                TargetDevicePath = "/dev/old"
            };

        var result = await service.ExecuteAsync(new UsbWriteExecutionRequest(preview, UserConfirmed: true), null, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessageKey.Should().Be("usbWrite.targetChanged");
        wwtService.AddedSources.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallWwtWhenPreviewIsConfirmed()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs, freeSpace: 512L * 1024L * 1024L);
        var wwtService = new FakeWwtService();
        var taskManager = new FakeTaskManager();
        var importHistoryRepository = new FakeImportHistoryRepository();
        var service = CreateService(
            [drive],
            wwtService: wwtService,
            taskManager: taskManager,
            readinessService: new FakeReadinessService(ReadinessState.Ready),
            importHistoryRepository: importHistoryRepository);
        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        var result = await service.ExecuteAsync(new UsbWriteExecutionRequest(preview, UserConfirmed: true), null, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.PostWriteReadinessState.Should().Be(ReadinessState.Ready);
        wwtService.AddedSources.Should().ContainSingle().Which.Should().Be(sourcePath);
        taskManager.Statuses.Should().Contain(ManagedTaskStatus.Completed);
        taskManager.ProgressValues.Should().Contain(progress => progress > 0.49 && progress < 0.51);
        importHistoryRepository.Entries.Should().ContainSingle(entry =>
            entry.GameId == "RMCP01"
            && entry.Operation == "usb-write"
            && entry.Success
            && entry.Message == "usbWrite.completed");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFailWhenPostWriteReadinessIsNotReady()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs, freeSpace: 512L * 1024L * 1024L);
        var taskManager = new FakeTaskManager();
        var service = CreateService(
            [drive],
            taskManager: taskManager,
            readinessService: new FakeReadinessService(ReadinessState.NotReady));
        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        var result = await service.ExecuteAsync(new UsbWriteExecutionRequest(preview, UserConfirmed: true), null, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessageKey.Should().Be("usbWrite.postWriteValidationFailed");
        result.PostWriteReadinessState.Should().Be(ReadinessState.NotReady);
        taskManager.Statuses.Should().Contain(ManagedTaskStatus.Failed);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private SafeUsbWriteWorkflowService CreateService(
        IReadOnlyList<UsbDrive> drives,
        FakeWwtService? wwtService = null,
        FakeExternalToolManager? externalToolManager = null,
        FakeDevicePermissionService? devicePermissionService = null,
        FakeTaskManager? taskManager = null,
        FakeNotificationService? notificationService = null,
        IWiiUsbReadinessService? readinessService = null,
        IImportHistoryRepository? importHistoryRepository = null)
    {
        return new SafeUsbWriteWorkflowService(
            new FakeUsbDriveService(drives),
            wwtService ?? new FakeWwtService(),
            externalToolManager ?? new FakeExternalToolManager(),
            devicePermissionService ?? new FakeDevicePermissionService(),
            taskManager ?? new FakeTaskManager(),
            notificationService ?? new FakeNotificationService(),
            readinessService,
            importHistoryRepository);
    }

    private string CreateSource(string fileName, int size)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, new byte[size]);
        return path;
    }

    private static UsbDrive CreateDrive(UsbDriveFileSystem fileSystem, long freeSpace)
    {
        var capacity = Math.Max(freeSpace, 1024L * 1024L * 1024L);
        return new UsbDrive(Guid.NewGuid(), "/dev/sdb1", "/media/wii", "Wii HD", fileSystem, capacity, capacity - freeSpace, freeSpace, true, true);
    }

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive> drives)
        {
            this.drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(drives);
        }
    }

    private sealed class FakeWwtService : IWwtService
    {
        private readonly IReadOnlyList<Game> games;

        public FakeWwtService(IReadOnlyList<Game>? games = null)
        {
            this.games = games ?? [];
        }

        public List<string> AddedSources { get; } = [];

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            return Task.FromResult(games);
        }

        public Task AddGameAsync(
            UsbDrive drive,
            string sourcePath,
            CancellationToken cancellationToken,
            IProgress<WiimmsOperationProgress>? progress = null,
            bool publishTask = true)
        {
            AddedSources.Add(sourcePath);
            progress?.Report(new WiimmsOperationProgress(50, "Copiando", sourcePath, null, null, null, null, IsIndeterminate: false));
            return Task.CompletedTask;
        }

        public Task RemoveGameAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private static Game CreateGame(string gameId)
    {
        return new Game(
            new WiiStudio.Domain.ValueObjects.GameId(gameId),
            "Mario Kart Wii",
            Region.Unknown,
            DiscType.Wii,
            FileFormat.Wbfs,
            $"/games/{gameId}.wbfs",
            1024,
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeExternalToolManager : IExternalToolManager
    {
        public Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ExternalToolStatus>>([CreateStatus("wiimms-wwt")]);
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
            return Task.FromResult(CreateStatus(request.ToolId));
        }

        private static ExternalToolStatus CreateStatus(string toolId)
        {
            return new ExternalToolStatus(toolId, toolId, ExternalToolState.Configured, "/usr/bin/wwt", "3.5", "test", null, DateTimeOffset.UtcNow);
        }
    }

    private sealed class FakeDevicePermissionService : IDevicePermissionService
    {
        public Task<DevicePermissionStatus> CheckAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DevicePermissionStatus(
                OSPlatformKind.Linux,
                PrivilegedAccessState.Authorized,
                CanRetry: false,
                RequiresUserAction: false,
                "ok",
                "ok",
                "ok"));
        }

        public Task<DevicePermissionStatus> RequestAuthorizationAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default)
        {
            return CheckAsync(request, cancellationToken);
        }
    }

    private sealed class FakeTaskManager : ITaskManager
    {
        public List<ManagedTaskStatus> Statuses { get; } = [];

        public List<double> ProgressValues { get; } = [];

        public Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Statuses.Add(ManagedTaskStatus.Queued);
            return Task.FromResult(Guid.NewGuid());
        }

        public Task UpdateProgressAsync(Guid taskId, double progress, ManagedTaskStatus status, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Statuses.Add(status);
            ProgressValues.Add(progress);
            return Task.CompletedTask;
        }

        public Task CompleteAsync(Guid taskId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Statuses.Add(ManagedTaskStatus.Completed);
            return Task.CompletedTask;
        }

        public Task FailAsync(Guid taskId, string errorMessage, CancellationToken cancellationToken)
        {
            Statuses.Add(ManagedTaskStatus.Failed);
            return Task.CompletedTask;
        }

        public Task PauseAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ResumeAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task CancelAsync(Guid taskId, CancellationToken cancellationToken)
        {
            Statuses.Add(ManagedTaskStatus.Cancelled);
            return Task.CompletedTask;
        }

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

    private sealed class FakeReadinessService : IWiiUsbReadinessService
    {
        private readonly ReadinessState state;

        public FakeReadinessService(ReadinessState state)
        {
            this.state = state;
        }

        public Task<WiiUsbReadinessResult> CheckAsync(Guid driveId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new WiiUsbReadinessResult(
                driveId.ToString(),
                state,
                [new WiiUsbReadinessCheckItem("post-write", "Pós-escrita", state, state.ToString(), null, null)],
                DateTimeOffset.UtcNow));
        }

        public Task ExportAsync(WiiUsbReadinessResult result, string outputFile, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeImportHistoryRepository : IImportHistoryRepository
    {
        public List<ImportHistoryEntry> Entries { get; } = [];

        public Task AppendAsync(ImportHistoryEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ImportHistoryEntry>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ImportHistoryEntry>>(Entries.Take(limit).ToList());
        }
    }
}
