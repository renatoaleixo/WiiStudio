using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class CoverAssetBulkActionService : ICoverAssetBulkActionService
{
    private readonly IAssetManager _assetManager;
    private readonly ITaskManager _taskManager;
    private readonly INotificationService _notificationService;

    public CoverAssetBulkActionService(
        IAssetManager assetManager,
        ITaskManager taskManager,
        INotificationService notificationService)
    {
        _assetManager = assetManager;
        _taskManager = taskManager;
        _notificationService = notificationService;
    }

    public async Task<CoverAssetBulkActionResult> ExecuteAsync(
        CoverAssetBulkActionRequest request,
        IProgress<CoverAssetBulkActionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var gameIds = request.GameIds
            .Select(GameIdValidation.Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var assetKinds = request.AssetKinds.Count == 0
            ? Enum.GetValues<CoverAssetKind>()
            : request.AssetKinds.Distinct().ToArray();

        var taskId = await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Download, "Assets em lote", $"{gameIds.Length} jogo(s), {assetKinds.Length} tipo(s)."),
            cancellationToken);

        var downloaded = 0;
        var fromCache = 0;
        var cleared = 0;
        var missing = 0;
        var failed = 0;
        var invalid = 0;
        var processedGames = 0;
        var cancelled = false;

        foreach (var gameId in gameIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancelled = true;
                break;
            }

            foreach (var kind in assetKinds)
            {
                progress?.Report(new CoverAssetBulkActionProgress(gameIds.Length, processedGames, gameId, kind, "running"));
                await _taskManager.UpdateProgressAsync(
                    taskId,
                    gameIds.Length == 0 ? 0 : (double)processedGames / gameIds.Length,
                    ManagedTaskStatus.Running,
                    cancellationToken);

                try
                {
                    var key = new AssetKey(gameId, kind);
                    if (request.ClearBeforeDownload || request.Mode == CoverAssetBulkActionMode.ClearCache)
                    {
                        if (await _assetManager.RemoveAsync(key, cancellationToken))
                        {
                            cleared++;
                        }
                    }

                    if (request.Mode == CoverAssetBulkActionMode.ClearCache)
                    {
                        continue;
                    }

                    var forceRefresh = request.ForceRefresh ||
                        request.Mode is CoverAssetBulkActionMode.ForceRefresh
                            or CoverAssetBulkActionMode.ReDownload
                            or CoverAssetBulkActionMode.ChangeLanguage;
                    var allowProvider = request.Mode != CoverAssetBulkActionMode.ValidateCache;
                    var asset = await _assetManager.GetAsync(new AssetRequest(key, allowProvider, forceRefresh), cancellationToken);
                    if (asset is null)
                    {
                        missing++;
                        continue;
                    }

                    if (!File.Exists(asset.LocalPath) || asset.SizeInBytes <= 0)
                    {
                        invalid++;
                        continue;
                    }

                    if (asset.Source == CoverAssetSource.Provider)
                    {
                        downloaded++;
                    }
                    else
                    {
                        fromCache++;
                    }
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                    break;
                }
                catch
                {
                    failed++;
                }
            }

            processedGames++;
        }

        var result = new CoverAssetBulkActionResult(
            gameIds.Length,
            gameIds.Length * assetKinds.Length,
            downloaded,
            fromCache,
            cleared,
            missing,
            failed,
            invalid,
            cancelled);

        if (cancelled)
        {
            await _taskManager.CancelAsync(taskId, CancellationToken.None);
        }
        else if (failed > 0 || invalid > 0)
        {
            await _taskManager.FailAsync(taskId, CreateMessage(result), CancellationToken.None);
        }
        else
        {
            await _taskManager.CompleteAsync(taskId, CancellationToken.None);
        }

        await _notificationService.PublishAsync(
            NotificationMessage.Create(
                failed > 0 || invalid > 0 ? NotificationType.Warning : NotificationType.Success,
                "Assets em lote concluído",
                CreateMessage(result),
                groupKey: "cover-asset-bulk"),
            CancellationToken.None);

        return result;
    }

    private static string CreateMessage(CoverAssetBulkActionResult result)
    {
        return $"Baixados: {result.Downloaded}; cache: {result.FromCache}; removidos: {result.Cleared}; ausentes: {result.Missing}; inválidos: {result.Invalid}; falhas: {result.Failed}.";
    }
}
