using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class ApplicationUpdateService : IApplicationUpdateService
{
    private readonly IApplicationUpdateProvider _updateProvider;
    private readonly IApplicationUpdateStateStore _stateStore;
    private readonly IApplicationUpdateInstaller _installer;
    private readonly INotificationService _notificationService;
    private readonly ITaskManager _taskManager;

    public ApplicationUpdateService(
        IApplicationUpdateProvider updateProvider,
        IApplicationUpdateStateStore stateStore,
        IApplicationUpdateInstaller installer,
        INotificationService notificationService,
        ITaskManager taskManager)
    {
        _updateProvider = updateProvider;
        _stateStore = stateStore;
        _installer = installer;
        _notificationService = notificationService;
        _taskManager = taskManager;
    }

    public async Task<ApplicationUpdateInfo> CheckAsync(CancellationToken cancellationToken)
    {
        var state = await _stateStore.LoadAsync(cancellationToken);
        var checkedAt = DateTimeOffset.UtcNow;
        var update = await _updateProvider.CheckAsync(state.CurrentVersion, cancellationToken)
            ?? new ApplicationUpdateInfo(state.CurrentVersion, state.CurrentVersion, IsAvailable: false, checkedAt);

        update = update with
        {
            CurrentVersion = state.CurrentVersion,
            IsAvailable = IsNewer(update.Version, state.CurrentVersion),
            CheckedAt = checkedAt
        };
        await _stateStore.SaveAsync(state with { LastCheckedAt = checkedAt }, cancellationToken);

        if (update.IsAvailable)
        {
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Information, "Update available", $"WiiStudio {update.Version} is available."),
                cancellationToken);
        }

        return update;
    }

    public async Task DownloadAsync(ApplicationUpdateInfo update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!update.IsAvailable)
        {
            throw new InvalidOperationException("No application update is available for download.");
        }

        await _taskManager.EnqueueAsync(new TaskRequest(ManagedTaskType.Update, $"Download WiiStudio {update.Version}"), cancellationToken);
        var package = await _updateProvider.DownloadAsync(update, cancellationToken)
            ?? throw new InvalidOperationException("Update provider did not return a package.");
        if (!IsPackageValid(package))
        {
            throw new InvalidOperationException("Update package failed integrity validation.");
        }

        var state = await _stateStore.LoadAsync(cancellationToken);
        await _stateStore.SaveAsync(state with
        {
            PendingVersion = package.Info.Version,
            PackagePath = package.PackagePath,
            IsDownloaded = true,
            IsValidated = true,
            RestartPending = false
        }, cancellationToken);
        await _notificationService.PublishAsync(
            NotificationMessage.Create(NotificationType.Success, "Update downloaded", $"WiiStudio {package.Info.Version} is ready to install.", isPersistent: true),
            cancellationToken);
    }

    public async Task InstallAsync(CancellationToken cancellationToken)
    {
        var state = await _stateStore.LoadAsync(cancellationToken);
        if (!state.IsDownloaded || !state.IsValidated || string.IsNullOrWhiteSpace(state.PackagePath) || string.IsNullOrWhiteSpace(state.PendingVersion))
        {
            throw new InvalidOperationException("No validated application update is ready to install.");
        }

        var packageInfo = new ApplicationUpdateInfo(
            state.PendingVersion,
            state.CurrentVersion,
            IsAvailable: true,
            DateTimeOffset.UtcNow);
        var package = new ApplicationUpdatePackage(packageInfo, state.PackagePath);

        await _taskManager.EnqueueAsync(new TaskRequest(ManagedTaskType.Update, $"Install WiiStudio {state.PendingVersion}"), cancellationToken);
        await _installer.InstallAsync(package, cancellationToken);

        await _stateStore.SaveAsync(state with
        {
            CurrentVersion = state.PendingVersion,
            PreviousVersion = state.CurrentVersion,
            PendingVersion = null,
            IsDownloaded = false,
            IsValidated = false,
            RestartPending = true,
            LastInstalledAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await _notificationService.PublishAsync(
            NotificationMessage.Create(NotificationType.Persistent, "Restart required", "Restart WiiStudio to finish installing the update.", isPersistent: true),
            cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        var state = await _stateStore.LoadAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(state.PreviousVersion))
        {
            return;
        }

        await _installer.RollbackAsync(state, cancellationToken);
        await _stateStore.SaveAsync(state with
        {
            CurrentVersion = state.PreviousVersion,
            PreviousVersion = state.CurrentVersion,
            PendingVersion = null,
            IsDownloaded = false,
            IsValidated = false,
            RestartPending = false,
            LastInstalledAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await _notificationService.PublishAsync(
            NotificationMessage.Create(NotificationType.Warning, "Update rolled back", "WiiStudio was restored to the previous version.", isPersistent: true),
            cancellationToken);
    }

    private static bool IsPackageValid(ApplicationUpdatePackage package)
    {
        if (string.IsNullOrWhiteSpace(package.PackagePath))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(package.Info.Checksum) &&
            !string.Equals(package.Info.Checksum, package.Checksum, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(package.Info.Signature) &&
            !string.Equals(package.Info.Signature, package.Signature, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static bool IsNewer(string latestVersion, string currentVersion)
    {
        if (Version.TryParse(latestVersion, out var latest) && Version.TryParse(currentVersion, out var current))
        {
            return latest > current;
        }

        return string.Compare(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
    }
}
