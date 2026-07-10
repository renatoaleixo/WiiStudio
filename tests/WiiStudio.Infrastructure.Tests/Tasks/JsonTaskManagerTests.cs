using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Tasks;

namespace WiiStudio.Infrastructure.Tests.Tasks;

public sealed class JsonTaskManagerTests
{
    [Fact]
    public async Task EnqueueAsync_ShouldPersistQueuedTaskAndPublishNotification()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var notifications = new FakeNotificationService();
        var manager = fixture.CreateManager(notifications);

        var taskId = await manager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Import, "Import game", "Mario Kart", ManagedTaskPriority.High),
            CancellationToken.None);

        var activeTasks = await manager.GetActiveTasksAsync(CancellationToken.None);
        var task = activeTasks.Should().ContainSingle().Subject;
        task.Id.Should().Be(taskId);
        task.Status.Should().Be(ManagedTaskStatus.Queued);
        task.Priority.Should().Be(ManagedTaskPriority.High);
        notifications.Messages.Should().ContainSingle().Which.Title.Should().Be("Import game");
    }

    [Fact]
    public async Task GetActiveTasksAsync_ShouldOrderByPriorityThenCreatedAt()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();

        await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Import, "Normal", Priority: ManagedTaskPriority.Normal), CancellationToken.None);
        await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Export, "High", Priority: ManagedTaskPriority.High), CancellationToken.None);

        var activeTasks = await manager.GetActiveTasksAsync(CancellationToken.None);

        activeTasks.Select(task => task.Title).Should().Equal("High", "Normal");
    }

    [Fact]
    public async Task PauseAsync_ShouldMoveTaskToPaused()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();
        var taskId = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Download, "Download covers"), CancellationToken.None);

        await manager.PauseAsync(taskId, CancellationToken.None);

        var task = (await manager.GetActiveTasksAsync(CancellationToken.None)).Should().ContainSingle().Subject;
        task.Status.Should().Be(ManagedTaskStatus.Paused);
    }

    [Fact]
    public async Task ResumeAsync_ShouldMovePausedTaskBackToQueued()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();
        var taskId = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Download, "Download covers"), CancellationToken.None);
        await manager.PauseAsync(taskId, CancellationToken.None);

        await manager.ResumeAsync(taskId, CancellationToken.None);

        var task = (await manager.GetActiveTasksAsync(CancellationToken.None)).Should().ContainSingle().Subject;
        task.Status.Should().Be(ManagedTaskStatus.Queued);
    }

    [Fact]
    public async Task CancelAsync_ShouldMoveTaskToHistory()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();
        var taskId = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Verification, "Verify game"), CancellationToken.None);

        await manager.CancelAsync(taskId, CancellationToken.None);

        (await manager.GetActiveTasksAsync(CancellationToken.None)).Should().BeEmpty();
        var history = await manager.GetHistoryAsync(CancellationToken.None);
        var task = history.Should().ContainSingle().Subject;
        task.Status.Should().Be(ManagedTaskStatus.Cancelled);
        task.FinishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProgressAsync_ShouldPersistRunningProgress()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();
        var taskId = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Download, "Download covers"), CancellationToken.None);

        await manager.UpdateProgressAsync(taskId, 0.5, ManagedTaskStatus.Running, CancellationToken.None);

        var task = (await manager.GetActiveTasksAsync(CancellationToken.None)).Should().ContainSingle().Subject;
        task.Status.Should().Be(ManagedTaskStatus.Running);
        task.Progress.Should().Be(0.5);
    }

    [Fact]
    public async Task CompleteAsync_ShouldMoveTaskToHistory()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();
        var taskId = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Update, "Update GameTDB"), CancellationToken.None);

        await manager.CompleteAsync(taskId, CancellationToken.None);

        (await manager.GetActiveTasksAsync(CancellationToken.None)).Should().BeEmpty();
        var task = (await manager.GetHistoryAsync(CancellationToken.None)).Should().ContainSingle().Subject;
        task.Status.Should().Be(ManagedTaskStatus.Completed);
        task.Progress.Should().Be(1);
    }

    [Fact]
    public async Task FailAsync_ShouldPersistErrorAndPublishErrorNotification()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var notifications = new FakeNotificationService();
        var manager = fixture.CreateManager(notifications);
        var taskId = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Update, "Update GameTDB"), CancellationToken.None);

        await manager.FailAsync(taskId, "boom", CancellationToken.None);

        var task = (await manager.GetHistoryAsync(CancellationToken.None)).Should().ContainSingle().Subject;
        task.Status.Should().Be(ManagedTaskStatus.Failed);
        task.ErrorMessage.Should().Be("boom");
        notifications.Messages.Should().Contain(message => message.Type == NotificationType.Error && message.Message == "boom");
    }

    [Fact]
    public async Task CancelAsync_ShouldRejectMissingTask()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();

        var act = () => manager.CancelAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task EnqueueAsync_ShouldRejectInvalidRequest()
    {
        using var fixture = TemporaryTaskManagerFixture.Create();
        var manager = fixture.CreateManager();

        var act = () => manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Import, " "), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldRespectHistoryLimit()
    {
        using var fixture = TemporaryTaskManagerFixture.Create(historyLimit: 2);
        var manager = fixture.CreateManager();
        var first = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Import, "One"), CancellationToken.None);
        var second = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Import, "Two"), CancellationToken.None);
        var third = await manager.EnqueueAsync(new TaskRequest(ManagedTaskType.Import, "Three"), CancellationToken.None);

        await manager.CancelAsync(first, CancellationToken.None);
        await manager.CancelAsync(second, CancellationToken.None);
        await manager.CancelAsync(third, CancellationToken.None);

        var history = await manager.GetHistoryAsync(CancellationToken.None);
        history.Should().HaveCount(2);
        history.Select(task => task.Title).Should().Equal("Three", "Two");
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

    private sealed class TemporaryTaskManagerFixture : IDisposable
    {
        private TemporaryTaskManagerFixture(string rootDirectory, int historyLimit)
        {
            RootDirectory = rootDirectory;
            HistoryLimit = historyLimit;
        }

        public string RootDirectory { get; }

        public int HistoryLimit { get; }

        public static TemporaryTaskManagerFixture Create(int historyLimit = 500)
        {
            return new TemporaryTaskManagerFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-tasks-{Guid.NewGuid():N}"), historyLimit);
        }

        public JsonTaskManager CreateManager(FakeNotificationService? notificationService = null)
        {
            return new JsonTaskManager(
                new TaskManagerOptions
                {
                    FilePath = Path.Combine(RootDirectory, "tasks.json"),
                    HistoryLimit = HistoryLimit
                },
                notificationService ?? new FakeNotificationService());
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
