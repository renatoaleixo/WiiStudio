using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class GameTdbUpdateService : IGameTdbUpdateService
{
    private readonly IGameTdbUpdateSource _updateSource;
    private readonly IGameTdbUpdateStateStore _stateStore;
    private readonly IMetadataCacheService _metadataCacheService;
    private readonly IAssetManager _assetManager;

    public GameTdbUpdateService(
        IGameTdbUpdateSource updateSource,
        IGameTdbUpdateStateStore stateStore,
        IMetadataCacheService metadataCacheService,
        IAssetManager assetManager)
    {
        _updateSource = updateSource;
        _stateStore = stateStore;
        _metadataCacheService = metadataCacheService;
        _assetManager = assetManager;
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

        return new UpdateCheckResult(isAvailable, state.InstalledVersion, latest, checkedAt, message);
    }

    public async Task<UpdateResult> UpdateAsync(GameTdbUpdateOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        var state = await _stateStore.LoadAsync(cancellationToken);
        var latest = await _updateSource.GetLatestAsync(cancellationToken);
        if (latest is null)
        {
            return CreateResult(GameTdbUpdateStatus.MirrorUnavailable, state, 0, "No GameTDB mirror is available.");
        }

        if (!options.Force && !IsNewer(latest.Version, state.InstalledVersion))
        {
            return CreateResult(GameTdbUpdateStatus.UpToDate, state, 0, "GameTDB is already up to date.");
        }

        try
        {
            var package = await _updateSource.DownloadAsync(latest, options, cancellationToken);
            if (package is null)
            {
                return CreateResult(GameTdbUpdateStatus.MirrorUnavailable, state, 0, "The selected mirror did not return an update package.");
            }

            if (!HasValidChecksum(package))
            {
                return CreateResult(GameTdbUpdateStatus.InvalidChecksum, state, 0, "The update package checksum is invalid.");
            }

            foreach (var metadata in package.Metadata)
            {
                await _metadataCacheService.SaveAsync(metadata with { GameTdbVersion = package.Info.Version }, cancellationToken);
            }

            await _assetManager.RebuildIndexAsync(cancellationToken);

            var updatedState = new GameTdbUpdateState(
                InstalledVersion: package.Info.Version,
                PreviousVersion: state.InstalledVersion,
                LastCheckedAt: DateTimeOffset.UtcNow,
                LastUpdatedAt: DateTimeOffset.UtcNow,
                LastMirror: package.Info.Mirror);
            await _stateStore.SaveAsync(updatedState, cancellationToken);

            return CreateResult(GameTdbUpdateStatus.Applied, updatedState, package.Metadata.Count, "GameTDB update applied.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return CreateResult(GameTdbUpdateStatus.Failed, state, 0, exception.Message);
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
}
