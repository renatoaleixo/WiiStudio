using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IBackupService
{
    Task<BackupPreview> AnalyzeAsync(CancellationToken cancellationToken);

    Task CreateAsync(BackupOptions options, string outputFile, CancellationToken cancellationToken);

    Task RestoreAsync(string backupFile, RestoreOptions options, CancellationToken cancellationToken);
}
