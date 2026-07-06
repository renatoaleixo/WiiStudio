using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class StatusBarServiceTests
{
    [Fact]
    public async Task RefreshAsync_ShouldExposeActiveDeviceAndFreeSpace()
    {
        var service = CreateService(devices: [CreateDevice("Wii HD", DeviceStatus.Available, freeSpace: 2048)]);

        var info = await service.RefreshAsync(CancellationToken.None);

        info.ActiveDevice.Should().Be("Wii HD");
        info.FileSystem.Should().Be("Wbfs");
        info.FreeSpace.Should().Be("2 KB");
        info.State.Should().Be(StatusBarState.Ready);
    }

    [Fact]
    public async Task RefreshAsync_ShouldSummarizeTasksAndNotifications()
    {
        var service = CreateService(
            tasks: [CreateTask()],
            notifications: [CreateNotification("Warning"), CreateNotification("Dismissed", dismissed: true)]);

        var info = await service.RefreshAsync(CancellationToken.None);

        info.ActiveTasks.Should().Be(1);
        info.PendingNotifications.Should().Be(1);
        info.State.Should().Be(StatusBarState.Warning);
    }

    [Fact]
    public async Task RefreshAsync_ShouldWarnWhenDeviceIsIncompatible()
    {
        var service = CreateService(devices: [CreateDevice("Unknown", DeviceStatus.Incompatible)]);

        var info = await service.RefreshAsync(CancellationToken.None);

        info.State.Should().Be(StatusBarState.Warning);
        info.Message.Should().Be("Atenção necessária.");
    }

    private static StatusBarService CreateService(
        IReadOnlyList<DeviceInfo>? devices = null,
        IReadOnlyList<TaskInfo>? tasks = null,
        IReadOnlyList<NotificationMessage>? notifications = null)
    {
        return new StatusBarService(
            new FakeDeviceManagerService(devices ?? []),
            new FakeTaskManager(tasks ?? []),
            new FakeNotificationService(notifications ?? []));
    }

    private static DeviceInfo CreateDevice(string name, DeviceStatus status, long freeSpace = 0)
    {
        return new DeviceInfo(
            Guid.NewGuid().ToString(),
            name,
            "/dev/sdb1",
            "/media/wii",
            "Unknown",
            name,
            Serial: null,
            "USB",
            UsbDriveFileSystem.Wbfs,
            Capacity: 4096,
            UsedSpace: 4096 - freeSpace,
            freeSpace,
            status,
            status != DeviceStatus.Incompatible,
            DateTimeOffset.UtcNow);
    }

    private static TaskInfo CreateTask()
    {
        return new TaskInfo(
            Guid.NewGuid(),
            ManagedTaskType.Import,
            ManagedTaskStatus.Running,
            "Import",
            Description: null,
            ManagedTaskPriority.Normal,
            0.5,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
    }

    private static NotificationMessage CreateNotification(string title, bool dismissed = false)
    {
        return NotificationMessage.Create(NotificationType.Warning, title, "Message") with { IsDismissed = dismissed };
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
