using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDiagnosticsRepairService
{
    Task<IReadOnlyList<DiagnosticRepairAction>> GetAvailableActionsAsync(
        DiagnosticsCheckResult item,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DiagnosticRepairHistoryEntry>> GetHistoryAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    Task<DiagnosticRepairResult> ExecuteAsync(
        DiagnosticRepairRequest request,
        CancellationToken cancellationToken = default);
}
