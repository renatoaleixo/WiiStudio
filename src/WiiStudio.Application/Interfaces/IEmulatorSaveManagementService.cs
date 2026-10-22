using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IEmulatorSaveManagementService
{
    Task<SaveLocationResult> LocateSavesAsync(string gameId, string emulatorId = "dolphin", CancellationToken cancellationToken = default);
    Task OpenSavesFolderAsync(string gameId, bool userConfirmed, CancellationToken cancellationToken = default);
    Task<SaveBackupResult> BackupSavesAsync(SaveBackupRequest request, CancellationToken cancellationToken = default);
}
