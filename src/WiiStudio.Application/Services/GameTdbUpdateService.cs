using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class GameTdbUpdateService : IGameTdbUpdateService
{
    private readonly IGameTdbUpdateSource _updateSource;
    private readonly IGameTdbUpdateStateStore _stateStore;
    private readonly IMetadataCacheService _metadataCacheService;
    private readonly IAssetManager _assetManager;
    private readonly INotificationService _notificationService;
    private readonly ITaskManager _taskManager;

    public GameTdbUpdateService(
        IGameTdbUpdateSource updateSource,
        IGameTdbUpdateStateStore stateStore,
        IMetadataCacheService metadataCacheService,
        IAssetManager assetManager,
        INotificationService notificationService,
        ITaskManager taskManager)
    {
        _updateSource = updateSource;
        _stateStore = stateStore;
        _metadataCacheService = metadataCacheService;
        _assetManager = assetManager;
        _notificationService = notificationService;
        _taskManager = taskManager;
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var checkedAt = DateTimeOffset.UtcNow;
        var state = await _stateStore.LoadAsync(cancellationToken);
        var latest = await _updateSource.GetLatestAsync(cancellationToken);
        await _stateStore.SaveAsync(state with { LastCheckedAt = checkedAt, LastMirror = latest?.Mirror ?? state.LastMirror }, cancellationToken);

        if (latest is null)
        {
            return new UpdateCheckResult(false, state.InstalledVersion, null, checkedAt, "No GameTDB mirror is available.");
        }

        var isAvailable = IsNewer(latest.Version, state.InstalledVersion);
        var message = isAvailable ? "A new GameTDB update is available." : "GameTDB is up to date.";
        if (isAvailable)
        {
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Information, "Atualização GameTDB disponível", $"Versão {latest.Version} disponível."),
                cancellationToken);
        }

        return new UpdateCheckResult(isAvailable, state.InstalledVersion, latest, checkedAt, message);
    }

    public async Task<UpdateResult> UpdateAsync(GameTdbUpdateOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        var taskId = await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Update, "Atualizando GameTDB", "Baixando e validando base local de metadados."),
            cancellationToken);
        var state = await _stateStore.LoadAsync(cancellationToken);
        var latest = await _updateSource.GetLatestAsync(cancellationToken);
        if (latest is null)
        {
            var result = CreateResult(GameTdbUpdateStatus.MirrorUnavailable, state, 0, "No GameTDB mirror is available.");
            await CompleteTaskForResultAsync(taskId, result, cancellationToken);
            return result;
        }

        if (!options.Force && !IsNewer(latest.Version, state.InstalledVersion))
        {
            var result = CreateResult(GameTdbUpdateStatus.UpToDate, state, 0, "GameTDB is already up to date.");
            await CompleteTaskForResultAsync(taskId, result, cancellationToken);
            return result;
        }

        try
        {
            await _taskManager.UpdateProgressAsync(taskId, 0.25, ManagedTaskStatus.Running, cancellationToken);
            var package = await _updateSource.DownloadAsync(latest, options, cancellationToken);
            if (package is null)
            {
                var unavailableResult = CreateResult(GameTdbUpdateStatus.MirrorUnavailable, state, 0, "The selected mirror did not return an update package.");
                await CompleteTaskForResultAsync(taskId, unavailableResult, cancellationToken);
                return unavailableResult;
            }

            if (!HasValidChecksum(package))
            {
                var checksumResult = CreateResult(GameTdbUpdateStatus.InvalidChecksum, state, 0, "The update package checksum is invalid.");
                await CompleteTaskForResultAsync(taskId, checksumResult, cancellationToken);
                return checksumResult;
            }

            await _taskManager.UpdateProgressAsync(taskId, 0.65, ManagedTaskStatus.Running, cancellationToken);
            foreach (var metadata in package.Metadata)
            {
                await _metadataCacheService.SaveAsync(metadata with { GameTdbVersion = package.Info.Version }, cancellationToken);
            }

            await _assetManager.RebuildIndexAsync(cancellationToken);
            await _taskManager.UpdateProgressAsync(taskId, 0.9, ManagedTaskStatus.Running, cancellationToken);

            var updatedState = new GameTdbUpdateState(
                InstalledVersion: package.Info.Version,
                PreviousVersion: state.InstalledVersion,
                LastCheckedAt: DateTimeOffset.UtcNow,
                LastUpdatedAt: DateTimeOffset.UtcNow,
                LastMirror: package.Info.Mirror);
            await _stateStore.SaveAsync(updatedState, cancellationToken);

            var result = CreateResult(GameTdbUpdateStatus.Applied, updatedState, package.Metadata.Count, "GameTDB update applied.");
            await CompleteTaskForResultAsync(taskId, result, cancellationToken);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            var result = CreateResult(GameTdbUpdateStatus.Failed, state, 0, exception.Message);
            await CompleteTaskForResultAsync(taskId, result, cancellationToken);
            return result;
        }
    }

    public Task RollbackAsync(CancellationToken cancellationToken)
    {
        return _stateStore.RollbackAsync(cancellationToken);
    }

    private static bool HasValidChecksum(GameTdbUpdatePackage package)
    {
        return string.IsNullOrWhiteSpace(package.Info.Checksum)
            || string.Equals(package.Info.Checksum, package.Checksum, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNewer(string latestVersion, string? installedVersion)
    {
        if (string.IsNullOrWhiteSpace(installedVersion))
        {
            return true;
        }

        if (Version.TryParse(latestVersion, out var latest) && Version.TryParse(installedVersion, out var installed))
        {
            return latest > installed;
        }

        return string.Compare(latestVersion, installedVersion, StringComparison.OrdinalIgnoreCase) > 0;
    }

    private static UpdateResult CreateResult(
        GameTdbUpdateStatus status,
        GameTdbUpdateState state,
        int metadataUpdated,
        string message)
    {
        return new UpdateResult(status, state.InstalledVersion, state.PreviousVersion, metadataUpdated, DateTimeOffset.UtcNow, message);
    }

    private async Task CompleteTaskForResultAsync(Guid taskId, UpdateResult result, CancellationToken cancellationToken)
    {
        if (result.Status == GameTdbUpdateStatus.Applied)
        {
            await _taskManager.CompleteAsync(taskId, cancellationToken);
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Success, "GameTDB atualizado", result.Message, groupKey: "gametdb-update"),
                cancellationToken);
            return;
        }

        if (result.Status == GameTdbUpdateStatus.UpToDate)
        {
            await _taskManager.CompleteAsync(taskId, cancellationToken);
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Information, "GameTDB em dia", result.Message, groupKey: "gametdb-update"),
                cancellationToken);
            return;
        }

        await _taskManager.FailAsync(taskId, result.Message, cancellationToken);
        await _notificationService.PublishAsync(
            NotificationMessage.Create(NotificationType.Error, "Falha ao atualizar GameTDB", result.Message, isPersistent: true, groupKey: "gametdb-update"),
            cancellationToken);
    }
}
