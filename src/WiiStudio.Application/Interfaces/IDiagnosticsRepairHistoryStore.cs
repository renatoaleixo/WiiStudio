using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDiagnosticsRepairHistoryStore
{
    Task AppendAsync(DiagnosticRepairHistoryEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DiagnosticRepairHistoryEntry>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default);
}
