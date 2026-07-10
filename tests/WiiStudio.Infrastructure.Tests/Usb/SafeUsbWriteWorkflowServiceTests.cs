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
    public async Task ExecuteAsync_ShouldCallWwtWhenPreviewIsConfirmed()
    {
        var sourcePath = CreateSource("RMCP01.iso", 1024);
        var drive = CreateDrive(UsbDriveFileSystem.Wbfs, freeSpace: 512L * 1024L * 1024L);
        var wwtService = new FakeWwtService();
        var service = CreateService([drive], wwtService: wwtService);
        var preview = await service.CreatePreviewAsync(sourcePath, drive.Id, CancellationToken.None);

        var result = await service.ExecuteAsync(new UsbWriteExecutionRequest(preview, UserConfirmed: true), null, CancellationToken.None);

        result.Success.Should().BeTrue();
        wwtService.AddedSources.Should().ContainSingle().Which.Should().Be(sourcePath);
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
        FakeDevicePermissionService? devicePermissionService = null)
    {
        return new SafeUsbWriteWorkflowService(
            new FakeUsbDriveService(drives),
            wwtService ?? new FakeWwtService(),
            externalToolManager ?? new FakeExternalToolManager(),
            devicePermissionService ?? new FakeDevicePermissionService());
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
        public List<string> AddedSources { get; } = [];

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Game>>([]);
        }

        public Task AddGameAsync(UsbDrive drive, string sourcePath, CancellationToken cancellationToken)
        {
            AddedSources.Add(sourcePath);
            return Task.CompletedTask;
        }

        public Task RemoveGameAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
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
}
