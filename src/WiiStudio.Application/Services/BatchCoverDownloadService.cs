using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class BatchCoverDownloadService : IBatchCoverDownloadService
{
    private readonly ICoverDownloadService _coverDownloadService;
    private readonly INotificationService _notificationService;
    private readonly ITaskManager _taskManager;

    public BatchCoverDownloadService(
        ICoverDownloadService coverDownloadService,
        INotificationService notificationService,
        ITaskManager taskManager)
    {
        _coverDownloadService = coverDownloadService;
        _notificationService = notificationService;
        _taskManager = taskManager;
    }

    public async Task<CoverBatchDownloadSummary> DownloadAsync(
        IReadOnlyList<string> gameIds,
        IReadOnlyList<CoverAssetKind> assetKinds,
        bool forceRefresh,
        IProgress<CoverBatchProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        ArgumentNullException.ThrowIfNull(assetKinds);

        var normalizedGameIds = gameIds
            .Select(GameIdValidation.Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var selectedKinds = assetKinds.Count == 0
            ? Enum.GetValues<CoverAssetKind>()
            : assetKinds.Distinct().ToArray();

        await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Download, "Baixando capas", $"{normalizedGameIds.Length} jogo(s) selecionado(s)."),
            cancellationToken);

        var downloaded = 0;
        var fromCache = 0;
        var missing = 0;
        var failed = 0;
        var processedGames = 0;
        var cancelled = false;

        foreach (var gameId in normalizedGameIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancelled = true;
                break;
            }

            progress?.Report(new CoverBatchProgress(
                normalizedGameIds.Length,
                processedGames,
                gameId,
                null,
                null,
                CoverAssetViewState.Downloading));

            try
            {
                var result = await _coverDownloadService.DownloadAsync(
                    new CoverDownloadRequest([gameId], forceRefresh, selectedKinds),
                    cancellationToken);

                downloaded += result.Items.Count(item => item.Source == CoverAssetSource.Provider);
                fromCache += result.Items.Count(item => item.Source == CoverAssetSource.Cache);
                missing += result.Items.Count(item => item.Source == CoverAssetSource.Missing);

                foreach (var item in result.Items)
                {
                    progress?.Report(new CoverBatchProgress(
                        normalizedGameIds.Length,
                        processedGames + 1,
                        gameId,
                        null,
                        item.Kind,
                        MapState(item.Source)));
                }
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                break;
            }
            catch
            {
                failed += selectedKinds.Length;
                progress?.Report(new CoverBatchProgress(
                    normalizedGameIds.Length,
                    processedGames,
                    gameId,
                    null,
                    null,
                    CoverAssetViewState.Failed));
            }

            processedGames++;
        }

        var summary = new CoverBatchDownloadSummary(
            normalizedGameIds.Length,
            downloaded,
            fromCache,
            missing,
            failed,
            processedGames,
            cancelled);

        await _notificationService.PublishAsync(
            NotificationMessage.Create(
                cancelled ? NotificationType.Warning : NotificationType.Success,
                cancelled ? "Download de capas cancelado" : "Download de capas concluído",
                CreateMessage(summary),
                groupKey: "cover-download"),
            CancellationToken.None);

        return summary;
    }

    private static CoverAssetViewState MapState(CoverAssetSource source)
    {
        return source switch
        {
            CoverAssetSource.Provider => CoverAssetViewState.Available,
            CoverAssetSource.Cache => CoverAssetViewState.FromCache,
            CoverAssetSource.Missing => CoverAssetViewState.Missing,
            _ => CoverAssetViewState.Failed
        };
    }

    private static string CreateMessage(CoverBatchDownloadSummary summary)
    {
        return $"Baixadas: {summary.Downloaded}; cache: {summary.FromCache}; ausentes: {summary.Missing}; falhas: {summary.Failed}; jogos: {summary.ProcessedGames}/{summary.Requested}.";
    }
}
