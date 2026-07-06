using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class ApplicationUpdateServiceTests
{
    [Fact]
    public async Task CheckAsync_ShouldReportAvailableUpdateAndPublishNotification()
    {
        var provider = new FakeUpdateProvider
        {
            Update = new ApplicationUpdateInfo("1.1.0", "1.0.0", IsAvailable: true, DateTimeOffset.UtcNow)
        };
        var stateStore = new FakeStateStore(new ApplicationUpdateState("1.0.0"));
        var notifications = new FakeNotificationService();
        var service = CreateService(provider, stateStore, notifications: notifications);

        var update = await service.CheckAsync(CancellationToken.None);

        update.IsAvailable.Should().BeTrue();
        update.CurrentVersion.Should().Be("1.0.0");
        stateStore.State.LastCheckedAt.Should().NotBeNull();
        notifications.Messages.Should().ContainSingle(message => message.Title == "Update available");
    }

    [Fact]
    public async Task DownloadAsync_ShouldPersistValidatedPackageStateAndQueueTask()
    {
        var update = new ApplicationUpdateInfo("1.1.0", "1.0.0", IsAvailable: true, DateTimeOffset.UtcNow, Checksum: "abc", Signature: "sig");
        var provider = new FakeUpdateProvider
        {
            Package = new ApplicationUpdatePackage(update, "/tmp/update.zip", Signature: "sig", Checksum: "abc")
        };
        var stateStore = new FakeStateStore(new ApplicationUpdateState("1.0.0"));
        var tasks = new FakeTaskManager();
        var service = CreateService(provider, stateStore, taskManager: tasks);

        await service.DownloadAsync(update, CancellationToken.None);

        stateStore.State.PendingVersion.Should().Be("1.1.0");
        stateStore.State.PackagePath.Should().Be("/tmp/update.zip");
        stateStore.State.IsDownloaded.Should().BeTrue();
        stateStore.State.IsValidated.Should().BeTrue();
        tasks.Requests.Should().ContainSingle().Which.Type.Should().Be(ManagedTaskType.Update);
    }

    [Fact]
    public async Task DownloadAsync_ShouldRejectInvalidChecksum()
    {
        var update = new ApplicationUpdateInfo("1.1.0", "1.0.0", IsAvailable: true, DateTimeOffset.UtcNow, Checksum: "expected");
        var provider = new FakeUpdateProvider
        {
            Package = new ApplicationUpdatePackage(update, "/tmp/update.zip", Checksum: "actual")
        };
        var service = CreateService(provider, new FakeStateStore(new ApplicationUpdateState("1.0.0")));

        var act = () => service.DownloadAsync(update, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InstallAsync_ShouldInstallValidatedPackageAndSetRestartPending()
    {
        var stateStore = new FakeStateStore(new ApplicationUpdateState(
            "1.0.0",
            PendingVersion: "1.1.0",
            PackagePath: "/tmp/update.zip",
            IsDownloaded: true,
            IsValidated: true));
        var installer = new FakeInstaller();
        var service = CreateService(new FakeUpdateProvider(), stateStore, installer: installer);

        await service.InstallAsync(CancellationToken.None);

        installer.InstallCount.Should().Be(1);
        stateStore.State.CurrentVersion.Should().Be("1.1.0");
        stateStore.State.PreviousVersion.Should().Be("1.0.0");
        stateStore.State.RestartPending.Should().BeTrue();
    }

    [Fact]
    public async Task InstallAsync_ShouldRejectWhenNoValidatedPackageExists()
    {
        var service = CreateService(new FakeUpdateProvider(), new FakeStateStore(new ApplicationUpdateState("1.0.0")));

        var act = () => service.InstallAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RollbackAsync_ShouldRestorePreviousVersion()
    {
        var stateStore = new FakeStateStore(new ApplicationUpdateState("1.1.0", PreviousVersion: "1.0.0", RestartPending: true));
        var installer = new FakeInstaller();
        var service = CreateService(new FakeUpdateProvider(), stateStore, installer: installer);

        await service.RollbackAsync(CancellationToken.None);

        installer.RollbackCount.Should().Be(1);
        stateStore.State.CurrentVersion.Should().Be("1.0.0");
        stateStore.State.PreviousVersion.Should().Be("1.1.0");
        stateStore.State.RestartPending.Should().BeFalse();
    }

    private static ApplicationUpdateService CreateService(
        IApplicationUpdateProvider provider,
        IApplicationUpdateStateStore stateStore,
        IApplicationUpdateInstaller? installer = null,
        INotificationService? notifications = null,
        ITaskManager? taskManager = null)
    {
        return new ApplicationUpdateService(
            provider,
            stateStore,
            installer ?? new FakeInstaller(),
            notifications ?? new FakeNotificationService(),
            taskManager ?? new FakeTaskManager());
    }

    private sealed class FakeUpdateProvider : IApplicationUpdateProvider
    {
        public ApplicationUpdateInfo? Update { get; init; }

        public ApplicationUpdatePackage? Package { get; init; }

        public Task<ApplicationUpdateInfo?> CheckAsync(string currentVersion, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Update);
        }

        public Task<ApplicationUpdatePackage?> DownloadAsync(ApplicationUpdateInfo update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Package);
        }
    }

    private sealed class FakeStateStore : IApplicationUpdateStateStore
    {
        public FakeStateStore(ApplicationUpdateState state)
        {
            State = state;
        }

        public ApplicationUpdateState State { get; private set; }

        public Task<ApplicationUpdateState> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(State);
        }

        public Task SaveAsync(ApplicationUpdateState state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            State = state;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeInstaller : IApplicationUpdateInstaller
    {
        public int InstallCount { get; private set; }

        public int RollbackCount { get; private set; }

        public Task InstallAsync(ApplicationUpdatePackage package, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            InstallCount++;

            return Task.CompletedTask;
        }

        public Task RollbackAsync(ApplicationUpdateState state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RollbackCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public List<NotificationMessage> Messages { get; } = [];

        public Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Messages.Add(message);

            return Task.CompletedTask;
        }

        public Task DismissAsync(Guid notificationId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<NotificationMessage>> GetHistoryAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task ClearHistoryAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeTaskManager : ITaskManager
    {
        public List<TaskRequest> Requests { get; } = [];

        public Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);

            return Task.FromResult(Guid.NewGuid());
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
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
