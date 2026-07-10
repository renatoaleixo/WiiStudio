using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Usb;

namespace WiiStudio.Infrastructure.Tests.Usb;

public sealed class UsbFormatPreparationServiceTests
{
    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockInternalDisk()
    {
        var device = CreateDevice(isUsb: false, isRemovable: false, fileSystem: "FAT32");
        var service = CreateService([device]);

        var preview = await service.CreatePreviewAsync(new UsbFormatPreparationRequest(device.DeviceId, null, UsbPreparationMode.FormatAsWbfs));

        preview.IsBlocked.Should().BeTrue();
        preview.BlockReasonKey.Should().Be("usbFormat.systemOrInternalDiskBlocked");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateWbfsFolderForFat32Target()
    {
        var fileSystem = new FakeUsbPreparationFileSystem(canWrite: true);
        var device = CreateDevice(fileSystem: "FAT32", mountPath: "/media/wii");
        var service = CreateService([device], fileSystem: fileSystem);
        var preview = await service.CreatePreviewAsync(new UsbFormatPreparationRequest(device.DeviceId, null, UsbPreparationMode.PrepareFat32WbfsFolder));

        var result = await service.ExecuteAsync(new UsbFormatExecutionRequest(preview, string.Empty, UserConfirmed: true));

        result.Success.Should().BeTrue();
        result.ResultFileSystem.Should().Be("FAT32");
        fileSystem.CreatedDirectories.Should().Contain("/media/wii/wbfs");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRequireStrongPhraseForWbfs()
    {
        var executor = new FakeWiimmsFormatCommandExecutor(hasCatalog: true);
        var device = CreateDevice(fileSystem: "FAT32");
        var service = CreateService([device], executor: executor);
        var preview = await service.CreatePreviewAsync(new UsbFormatPreparationRequest(device.DeviceId, null, UsbPreparationMode.FormatAsWbfs));

        var result = await service.ExecuteAsync(new UsbFormatExecutionRequest(preview, "errado", UserConfirmed: true));

        result.Success.Should().BeFalse();
        result.ErrorMessageKey.Should().Be("usbFormat.strongConfirmationRequired");
        executor.Executed.Should().BeFalse();
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockWbfsWhenCommandIsNotCatalogued()
    {
        var device = CreateDevice(fileSystem: "FAT32");
        var service = CreateService([device], executor: new FakeWiimmsFormatCommandExecutor(hasCatalog: false));

        var preview = await service.CreatePreviewAsync(new UsbFormatPreparationRequest(device.DeviceId, null, UsbPreparationMode.FormatAsWbfs));

        preview.IsBlocked.Should().BeTrue();
        preview.BlockReasonKey.Should().Be("usbFormat.wbfsCommandNotCatalogued");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRunCataloguedWbfsExecutorAfterStrongConfirmation()
    {
        var executor = new FakeWiimmsFormatCommandExecutor(hasCatalog: true);
        var device = CreateDevice(fileSystem: "FAT32");
        var service = CreateService([device], executor: executor);
        var preview = await service.CreatePreviewAsync(new UsbFormatPreparationRequest(device.DeviceId, null, UsbPreparationMode.FormatAsWbfs));

        var result = await service.ExecuteAsync(new UsbFormatExecutionRequest(preview, UsbFormatPreparationService.WbfsConfirmationPhrase, UserConfirmed: true));

        result.Success.Should().BeTrue();
        result.ResultFileSystem.Should().Be("WBFS");
        executor.Executed.Should().BeTrue();
    }

    private static UsbFormatPreparationService CreateService(
        IReadOnlyList<UsbDeviceInfo> devices,
        FakeUsbPreparationFileSystem? fileSystem = null,
        FakeWiimmsFormatCommandExecutor? executor = null,
        FakeExternalToolManager? externalToolManager = null,
        FakeDevicePermissionService? permissionService = null,
        FakeTaskManager? taskManager = null,
        FakeNotificationService? notificationService = null,
        FakeReadinessService? readinessService = null)
    {
        return new UsbFormatPreparationService(
            new FakeUsbDeviceProvider(devices),
            externalToolManager ?? new FakeExternalToolManager(),
            permissionService ?? new FakeDevicePermissionService(),
            taskManager ?? new FakeTaskManager(),
            notificationService ?? new FakeNotificationService(),
            readinessService ?? new FakeReadinessService(),
            fileSystem ?? new FakeUsbPreparationFileSystem(canWrite: true),
            executor ?? new FakeWiimmsFormatCommandExecutor(hasCatalog: false));
    }

    private static UsbDeviceInfo CreateDevice(
        bool isUsb = true,
        bool isRemovable = true,
        string fileSystem = "FAT32",
        string mountPath = "/media/wii")
    {
        var partition = new UsbPartitionInfo(
            "partition-1",
            "/dev/sdb1",
            mountPath,
            fileSystem,
            64L * 1024L * 1024L * 1024L,
            32L * 1024L * 1024L * 1024L,
            IsWbfsCandidate: string.Equals(fileSystem, "WBFS", StringComparison.OrdinalIgnoreCase),
            IsWritable: true);

        return new UsbDeviceInfo(
            "device-1",
            "Wii USB",
            "/dev/sdb",
            mountPath,
            isUsb,
            isRemovable,
            64L * 1024L * 1024L * 1024L,
            [partition],
            new DevicePermissionStatus(OSPlatformKind.Linux, PrivilegedAccessState.Authorized, false, false, null, null, null));
    }

    private sealed class FakeUsbDeviceProvider : IUsbDeviceProvider
    {
        private readonly IReadOnlyList<UsbDeviceInfo> devices;

        public FakeUsbDeviceProvider(IReadOnlyList<UsbDeviceInfo> devices)
        {
            this.devices = devices;
        }

        public Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default) => Task.FromResult(devices);
    }

    private sealed class FakeUsbPreparationFileSystem : IUsbPreparationFileSystem
    {
        private readonly bool canWrite;

        public FakeUsbPreparationFileSystem(bool canWrite)
        {
            this.canWrite = canWrite;
        }

        public List<string> CreatedDirectories { get; } = [];

        public bool DirectoryExists(string path) => CreatedDirectories.Contains(path, StringComparer.OrdinalIgnoreCase);

        public void CreateDirectory(string path) => CreatedDirectories.Add(path);

        public bool CanWriteDirectory(string path) => canWrite;
    }

    private sealed class FakeWiimmsFormatCommandExecutor : IWiimmsFormatCommandExecutor
    {
        public FakeWiimmsFormatCommandExecutor(bool hasCatalog)
        {
            HasCataloguedFormatWbfsCommand = hasCatalog;
        }

        public bool HasCataloguedFormatWbfsCommand { get; }
        public bool Executed { get; private set; }

        public Task ExecuteFormatWbfsAsync(UsbFormatPreview preview, IProgress<UsbFormatProgress>? progress, CancellationToken cancellationToken)
        {
            Executed = true;
            return Task.CompletedTask;
        }

        public Task<bool> ValidateWbfsAsync(UsbFormatPreview preview, CancellationToken cancellationToken) => Task.FromResult(true);
    }

    private sealed class FakeExternalToolManager : IExternalToolManager
    {
        public Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ExternalToolStatus>>([CreateStatus("wiimms-wwt")]);

        public Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateStatus(toolId));

        public Task<ExternalToolInstallResult> InstallAsync(ExternalToolInstallRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExternalToolInstallResult.Failed(request.ToolId, "not-implemented"));

        public Task<ExternalToolStatus> ConfigureManualPathAsync(ExternalToolManualPathRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateStatus(request.ToolId));

        private static ExternalToolStatus CreateStatus(string toolId) =>
            new(toolId, toolId, ExternalToolState.Configured, "/usr/bin/wwt", "3.5", "test", null, DateTimeOffset.UtcNow);
    }

    private sealed class FakeDevicePermissionService : IDevicePermissionService
    {
        public Task<DevicePermissionStatus> CheckAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DevicePermissionStatus(OSPlatformKind.Linux, PrivilegedAccessState.Authorized, false, false, null, null, null));

        public Task<DevicePermissionStatus> RequestAuthorizationAsync(DevicePermissionRequest request, CancellationToken cancellationToken = default) =>
            CheckAsync(request, cancellationToken);
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

    private sealed class FakeReadinessService : IWiiUsbReadinessService
    {
        public Task<WiiUsbReadinessResult> CheckAsync(Guid driveId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new WiiUsbReadinessResult(driveId.ToString(), ReadinessState.Ready, [], DateTimeOffset.UtcNow));

        public Task ExportAsync(WiiUsbReadinessResult result, string outputFile, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
