using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Notifications;

namespace WiiStudio.Infrastructure.Tests.Notifications;

public sealed class JsonNotificationServiceTests
{
    [Fact]
    public async Task PublishAsync_ShouldPersistNotificationInHistory()
    {
        using var fixture = TemporaryNotificationFixture.Create();
        var service = fixture.CreateService();
        var notification = CreateNotification(NotificationType.Information, "Ready", "Library scan completed.");

        await service.PublishAsync(notification, CancellationToken.None);

        var history = await service.GetHistoryAsync(CancellationToken.None);
        history.Should().ContainSingle().Which.Should().Be(notification);
    }

    [Fact]
    public async Task PublishAsync_ShouldUpdateExistingNotificationWithSameId()
    {
        using var fixture = TemporaryNotificationFixture.Create();
        var service = fixture.CreateService();
        var notification = CreateNotification(NotificationType.Progress, "Import", "Starting.", progress: 0.1);

        await service.PublishAsync(notification, CancellationToken.None);
        await service.PublishAsync(notification with { Message = "Halfway.", Progress = 0.5 }, CancellationToken.None);

        var history = await service.GetHistoryAsync(CancellationToken.None);
        var updated = history.Should().ContainSingle().Subject;
        updated.Message.Should().Be("Halfway.");
        updated.Progress.Should().Be(0.5);
    }

    [Fact]
    public async Task DismissAsync_ShouldMarkNotificationAsDismissed()
    {
        using var fixture = TemporaryNotificationFixture.Create();
        var service = fixture.CreateService();
        var notification = CreateNotification(NotificationType.Warning, "Warning", "Check USB drive.");
        await service.PublishAsync(notification, CancellationToken.None);

        await service.DismissAsync(notification.Id, CancellationToken.None);

        var history = await service.GetHistoryAsync(CancellationToken.None);
        history.Should().ContainSingle().Which.IsDismissed.Should().BeTrue();
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnNewestFirst()
    {
        using var fixture = TemporaryNotificationFixture.Create();
        var service = fixture.CreateService();
        var older = CreateNotification(NotificationType.Information, "Older", "First.", createdAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        var newer = CreateNotification(NotificationType.Success, "Newer", "Second.", createdAt: DateTimeOffset.UtcNow);

        await service.PublishAsync(older, CancellationToken.None);
        await service.PublishAsync(newer, CancellationToken.None);

        var history = await service.GetHistoryAsync(CancellationToken.None);
        history.Select(notification => notification.Title).Should().Equal("Newer", "Older");
    }

    [Fact]
    public async Task PublishAsync_ShouldRespectHistoryLimit()
    {
        using var fixture = TemporaryNotificationFixture.Create(historyLimit: 2);
        var service = fixture.CreateService();

        await service.PublishAsync(CreateNotification(NotificationType.Information, "One", "1", createdAt: DateTimeOffset.UtcNow.AddMinutes(-2)), CancellationToken.None);
        await service.PublishAsync(CreateNotification(NotificationType.Information, "Two", "2", createdAt: DateTimeOffset.UtcNow.AddMinutes(-1)), CancellationToken.None);
        await service.PublishAsync(CreateNotification(NotificationType.Information, "Three", "3", createdAt: DateTimeOffset.UtcNow), CancellationToken.None);

        var history = await service.GetHistoryAsync(CancellationToken.None);
        history.Select(notification => notification.Title).Should().Equal("Three", "Two");
    }

    [Fact]
    public async Task ClearHistoryAsync_ShouldRemovePersistedNotifications()
    {
        using var fixture = TemporaryNotificationFixture.Create();
        var service = fixture.CreateService();
        await service.PublishAsync(CreateNotification(NotificationType.Information, "Ready", "Done."), CancellationToken.None);

        await service.ClearHistoryAsync(CancellationToken.None);

        var history = await service.GetHistoryAsync(CancellationToken.None);
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_ShouldRejectInvalidNotification()
    {
        using var fixture = TemporaryNotificationFixture.Create();
        var service = fixture.CreateService();
        var notification = CreateNotification(NotificationType.Progress, "Import", "Invalid.", progress: 1.5);

        var act = () => service.PublishAsync(notification, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static NotificationMessage CreateNotification(
        NotificationType type,
        string title,
        string message,
        DateTimeOffset? createdAt = null,
        double? progress = null)
    {
        return new NotificationMessage(
            Guid.NewGuid(),
            type,
            title,
            message,
            createdAt ?? DateTimeOffset.UtcNow,
            IsPersistent: false,
            IsDismissed: false,
            GroupKey: null,
            progress,
            Actions: null);
    }

    private sealed class TemporaryNotificationFixture : IDisposable
    {
        private TemporaryNotificationFixture(string rootDirectory, int historyLimit)
        {
            RootDirectory = rootDirectory;
            HistoryLimit = historyLimit;
        }

        public string RootDirectory { get; }

        public int HistoryLimit { get; }

        public static TemporaryNotificationFixture Create(int historyLimit = 200)
        {
            return new TemporaryNotificationFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-notifications-{Guid.NewGuid():N}"), historyLimit);
        }

        public JsonNotificationService CreateService()
        {
            return new JsonNotificationService(new NotificationStoreOptions
            {
                FilePath = Path.Combine(RootDirectory, "history.json"),
                HistoryLimit = HistoryLimit
            });
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
