using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task RefreshAsync_ShouldAggregateLibraryDevicesTasksAndNotifications()
    {
        var service = CreateService(
            games:
            [
                CreateGame("RMCP01", FileFormat.Wbfs, 1024),
                CreateGame("RSBP01", FileFormat.Iso, 2048)
            ],
            devices: [CreateDevice(freeSpace: 4096)],
            tasks: [CreateTask(ManagedTaskType.Conversion), CreateTask(ManagedTaskType.Download)],
            notifications: [CreateNotification("Ready"), CreateNotification("Hidden", dismissed: true)]);

        var data = await service.RefreshAsync(CancellationToken.None);

        data.TotalGames.Should().Be(2);
        data.GamesByFormat[FileFormat.Wbfs].Should().Be(1);
        data.GamesByFormat[FileFormat.Iso].Should().Be(1);
        data.LibrarySizeInBytes.Should().Be(3072);
        data.DeviceFreeSpaceInBytes.Should().Be(4096);
        data.ConnectedDevices.Should().Be(1);
        data.ActiveTasks.Should().Be(2);
        data.ActiveConversions.Should().Be(1);
        data.ActiveDownloads.Should().Be(1);
        data.RecentNotifications.Should().Be(1);
        data.Widgets.Should().HaveCount(4);
        data.QuickActions.Should().HaveCount(4);
    }

    [Fact]
    public async Task RefreshAsync_ShouldIncludeRecentActivities()
    {
        var service = CreateService(
            games: [],
            devices: [],
            tasks: [CreateTask(ManagedTaskType.Import, "Import Mario")],
            notifications: [CreateNotification("Warning")]);

        var data = await service.RefreshAsync(CancellationToken.None);

        data.Activities.Select(activity => activity.Title).Should().Contain(["Import Mario", "Warning"]);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnCachedDataAfterFirstLoad()
    {
        var library = new FakeGameLibraryService([CreateGame("RMCP01", FileFormat.Wbfs, 1024)]);
        var service = CreateService(gameLibraryService: library);

        await service.LoadAsync(CancellationToken.None);
        await service.LoadAsync(CancellationToken.None);

        library.GetGamesCalls.Should().Be(1);
    }

    private static DashboardService CreateService(
        IReadOnlyList<GameDto>? games = null,
        IReadOnlyList<DeviceInfo>? devices = null,
        IReadOnlyList<TaskInfo>? tasks = null,
        IReadOnlyList<NotificationMessage>? notifications = null,
        FakeGameLibraryService? gameLibraryService = null)
    {
        return new DashboardService(
            gameLibraryService ?? new FakeGameLibraryService(games ?? []),
            new FakeDeviceManagerService(devices ?? []),
            new FakeTaskManager(tasks ?? []),
            new FakeNotificationService(notifications ?? []));
    }

    private static GameDto CreateGame(string id, FileFormat format, long sizeInBytes)
    {
        return new GameDto(id, id, Region.Pal, DiscType.Wii, format, $"/games/{id}.wbfs", sizeInBytes, DateTimeOffset.UtcNow);
    }

    private static DeviceInfo CreateDevice(long freeSpace)
    {
        return new DeviceInfo(
            Guid.NewGuid().ToString(),
            "Wii HD",
            "/dev/sdb1",
            "/media/wii",
            "Unknown",
            "Wii HD",
            Serial: null,
            "USB",
            UsbDriveFileSystem.Wbfs,
            Capacity: 8192,
            UsedSpace: 4096,
            freeSpace,
            DeviceStatus.Available,
            IsWiiCompatible: true,
            DateTimeOffset.UtcNow);
    }

    private static TaskInfo CreateTask(ManagedTaskType type, string title = "Task")
    {
        return new TaskInfo(
            Guid.NewGuid(),
            type,
            ManagedTaskStatus.Running,
            title,
            Description: null,
            ManagedTaskPriority.Normal,
            Progress: 0.5,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
    }

    private static NotificationMessage CreateNotification(string title, bool dismissed = false)
    {
        return NotificationMessage.Create(NotificationType.Information, title, "Message") with { IsDismissed = dismissed };
    }

    private sealed class FakeGameLibraryService : IGameLibraryService
    {
        private readonly IReadOnlyList<GameDto> _games;

        public FakeGameLibraryService(IReadOnlyList<GameDto> games)
        {
            _games = games;
        }

        public int GetGamesCalls { get; private set; }

        public Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GetGamesCalls++;

            return Task.FromResult(_games);
        }

        public Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }

    private sealed class FakeDeviceManagerService : IDeviceManagerService
    {
        private readonly IReadOnlyList<DeviceInfo> _devices;

        public FakeDeviceManagerService(IReadOnlyList<DeviceInfo> devices)
        {
            _devices = devices;
        }

        public Task<DeviceManagerSnapshot> RefreshAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new DeviceManagerSnapshot(_devices, DeviceManagerState.Ready, DateTimeOffset.UtcNow));
        }

        public Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_devices);
        }

        public Task<DeviceInfo> EjectAsync(string deviceId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeTaskManager : ITaskManager
    {
        private readonly IReadOnlyList<TaskInfo> _tasks;

        public FakeTaskManager(IReadOnlyList<TaskInfo> tasks)
        {
            _tasks = tasks;
        }

        public Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task PauseAsync(Guid taskId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task ResumeAsync(Guid taskId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task CancelAsync(Guid taskId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<TaskInfo>> GetActiveTasksAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_tasks);
        }

        public Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_tasks);
        }
    }

    private sealed class FakeNotificationService : INotificationService
    {
        private readonly IReadOnlyList<NotificationMessage> _notifications;

        public FakeNotificationService(IReadOnlyList<NotificationMessage> notifications)
        {
            _notifications = notifications;
        }

        public Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task DismissAsync(Guid notificationId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<NotificationMessage>> GetHistoryAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_notifications);
        }

        public Task ClearHistoryAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
