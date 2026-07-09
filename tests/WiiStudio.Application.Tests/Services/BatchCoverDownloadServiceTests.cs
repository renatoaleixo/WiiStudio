using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class BatchCoverDownloadServiceTests
{
    [Fact]
    public async Task DownloadAsync_ShouldProcessMultipleGamesAndPublishSummary()
    {
        var coverDownload = new FakeCoverDownloadService();
        coverDownload.Results["RMCP01"] =
        [
            new CoverDownloadItem("RMCP01", CoverAssetKind.FrontCover, CoverAssetSource.Provider, "/covers/RMCP01.png", "downloaded"),
            new CoverDownloadItem("RMCP01", CoverAssetKind.BackCover, CoverAssetSource.Cache, "/covers-back/RMCP01.png", "cache")
        ];
        coverDownload.Results["RSPE01"] =
        [
            new CoverDownloadItem("RSPE01", CoverAssetKind.FrontCover, CoverAssetSource.Missing, null, "missing")
        ];
        var notifications = new FakeNotificationService();
        var tasks = new FakeTaskManager();
        var service = new BatchCoverDownloadService(coverDownload, notifications, tasks);
        var progress = new List<CoverBatchProgress>();

        var summary = await service.DownloadAsync(
            ["rmcp01", "rspe01"],
            [CoverAssetKind.FrontCover, CoverAssetKind.BackCover],
            forceRefresh: false,
            new InlineProgress<CoverBatchProgress>(progress.Add),
            CancellationToken.None);

        summary.Requested.Should().Be(2);
        summary.ProcessedGames.Should().Be(2);
        summary.Downloaded.Should().Be(1);
        summary.FromCache.Should().Be(1);
        summary.Missing.Should().Be(1);
        tasks.Requests.Should().ContainSingle().Which.Type.Should().Be(ManagedTaskType.Download);
        notifications.Messages.Should().ContainSingle(message => message.Type == NotificationType.Success);
        progress.Should().Contain(item => item.CurrentGameId == "RMCP01");
        coverDownload.Requests.Should().HaveCount(2);
    }

    [Fact]
    public async Task DownloadAsync_ShouldContinueWhenOneGameFails()
    {
        var coverDownload = new FakeCoverDownloadService();
        coverDownload.Failures.Add("RMCP01");
        coverDownload.Results["RSPE01"] =
        [
            new CoverDownloadItem("RSPE01", CoverAssetKind.FrontCover, CoverAssetSource.Provider, "/covers/RSPE01.png", "downloaded")
        ];
        var service = new BatchCoverDownloadService(coverDownload, new FakeNotificationService(), new FakeTaskManager());

        var summary = await service.DownloadAsync(
            ["RMCP01", "RSPE01"],
            [CoverAssetKind.FrontCover],
            forceRefresh: false,
            progress: null,
            CancellationToken.None);

        summary.ProcessedGames.Should().Be(2);
        summary.Downloaded.Should().Be(1);
        summary.Failed.Should().Be(1);
    }

    private sealed class FakeCoverDownloadService : ICoverDownloadService
    {
        public Dictionary<string, IReadOnlyList<CoverDownloadItem>> Results { get; } = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Failures { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<CoverDownloadRequest> Requests { get; } = [];

        public Task<CoverDownloadPreview> AnalyzeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<CoverDownloadResult> DownloadAsync(CoverDownloadRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            var gameId = request.GameIds.Single();
            if (Failures.Contains(gameId))
            {
                throw new InvalidOperationException("boom");
            }

            Results.TryGetValue(gameId, out var items);
            return Task.FromResult(new CoverDownloadResult(items ?? [], DateTimeOffset.UtcNow));
        }
    }

    private sealed class InlineProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;

        public InlineProgress(Action<T> handler)
        {
            _handler = handler;
        }

        public void Report(T value)
        {
            _handler(value);
        }
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

    private sealed class FakeTaskManager : ITaskManager
    {
        public List<TaskRequest> Requests { get; } = [];

        public Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(Guid.NewGuid());
        }

        public Task PauseAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ResumeAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task CancelAsync(Guid taskId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<TaskInfo>> GetActiveTasksAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TaskInfo>>([]);

        public Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TaskInfo>>([]);
    }
}
