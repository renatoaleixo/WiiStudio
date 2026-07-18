using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IImportHistoryRepository
{
    Task AppendAsync(ImportHistoryEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportHistoryEntry>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);
}
