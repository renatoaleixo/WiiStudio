using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ISaveBackupRepository
{
    Task AppendAsync(SaveBackupEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaveBackupEntry>> GetRecentAsync(string? gameId = null, int limit = 100, CancellationToken cancellationToken = default);
}
