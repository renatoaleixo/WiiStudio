using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class CoverAssetBulkActionServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"wiistudio-bulk-assets-{Guid.NewGuid():N}");

    [Fact]
    public async Task ExecuteAsync_ShouldDownloadSelectedAssetsAndCompleteTask()
    {
        var assets = new FakeAssetManager();
        assets.Results[(CoverAssetKind.FrontCover, "RMCP01")] = CreateAsset("RMCP01", CoverAssetKind.FrontCover, CoverAssetSource.Provider);
        var tasks = new FakeTaskManager();
        var notifications = new FakeNotificationService();
        var service = new CoverAssetBulkActionService(assets, tasks, notifications);

        var result = await service.ExecuteAsync(
            new CoverAssetBulkActionRequest(["rmcp01"], [CoverAssetKind.FrontCover], "PT", ForceRefresh: false, ClearBeforeDownload: false, CoverAssetBulkActionMode.DownloadMissing),
            progress: null,
            CancellationToken.None);

        result.Downloaded.Should().Be(1);
        tasks.Statuses.Should().Contain(ManagedTaskStatus.Completed);
        notifications.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldClearCacheWithoutDownloading()
    {
        var assets = new FakeAssetManager();
        assets.RemoveResults[(CoverAssetKind.FrontCover, "RMCP01")] = true;
        var service = new CoverAssetBulkActionService(assets, new FakeTaskManager(), new FakeNotificationService());

        var result = await service.ExecuteAsync(
            new CoverAssetBulkActionRequest(["RMCP01"], [CoverAssetKind.FrontCover], null, ForceRefresh: false, ClearBeforeDownload: false, CoverAssetBulkActionMode.ClearCache),
            progress: null,
            CancellationToken.None);

        result.Cleared.Should().Be(1);
        assets.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassForceRefreshLanguageAndSelectedKinds()
    {
        var assets = new FakeAssetManager();
        assets.Results[(CoverAssetKind.DiscArt, "RMCP01")] = CreateAsset("RMCP01", CoverAssetKind.DiscArt, CoverAssetSource.Cache);
        var service = new CoverAssetBulkActionService(assets, new FakeTaskManager(), new FakeNotificationService());

        await service.ExecuteAsync(
            new CoverAssetBulkActionRequest(["RMCP01"], [CoverAssetKind.DiscArt], "en", ForceRefresh: true, ClearBeforeDownload: false, CoverAssetBulkActionMode.ForceRefresh),
            progress: null,
            CancellationToken.None);

        assets.Requests.Should().ContainSingle();
        assets.Requests[0].Key.Kind.Should().Be(CoverAssetKind.DiscArt);
        assets.Requests[0].ForceRefresh.Should().BeTrue();
        assets.Requests[0].Language.Should().Be("EN");
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private AssetInfo CreateAsset(string gameId, CoverAssetKind kind, CoverAssetSource source)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, $"{gameId}-{kind}.png");
        File.WriteAllBytes(path, [1, 2, 3]);
        return new AssetInfo(new AssetKey(gameId, kind), path, 3, DateTimeOffset.UtcNow, source);
    }

    private sealed class FakeAssetManager : IAssetManager
    {
        public Dictionary<(CoverAssetKind Kind, string GameId), AssetInfo?> Results { get; } = [];

        public Dictionary<(CoverAssetKind Kind, string GameId), bool> RemoveResults { get; } = [];

        public List<AssetRequest> Requests { get; } = [];

        public Task<AssetInfo?> GetAsync(AssetRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Results.TryGetValue((request.Key.Kind, request.Key.GameId), out var asset);
            return Task.FromResult(asset);
        }

        public Task SaveAsync(AssetSaveRequest request, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> RemoveAsync(AssetKey key, CancellationToken cancellationToken)
        {
            RemoveResults.TryGetValue((key.Kind, key.GameId), out var removed);
            return Task.FromResult(removed);
        }

        public Task RebuildIndexAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<AssetStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new AssetStatistics(0, 0, new Dictionary<CoverAssetKind, int>(), null));
        }
    }

    private sealed class FakeTaskManager : ITaskManager
    {
        public List<ManagedTaskStatus> Statuses { get; } = [];

        public Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken)
        {
            Statuses.Add(ManagedTaskStatus.Queued);
            return Task.FromResult(Guid.NewGuid());
        }

        public Task UpdateProgressAsync(Guid taskId, double progress, ManagedTaskStatus status, CancellationToken cancellationToken)
        {
            Statuses.Add(status);
            return Task.CompletedTask;
        }

        public Task CompleteAsync(Guid taskId, CancellationToken cancellationToken)
        {
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
        public List<NotificationMessage> Messages { get; } = [];

        public Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task DismissAsync(Guid notificationId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<NotificationMessage>> GetHistoryAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<NotificationMessage>>(Messages);

        public Task ClearHistoryAsync(CancellationToken cancellationToken)
        {
            Messages.Clear();
            return Task.CompletedTask;
        }
    }
}
