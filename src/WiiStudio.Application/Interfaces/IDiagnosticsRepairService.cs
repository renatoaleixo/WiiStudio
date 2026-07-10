using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDiagnosticsRepairService
{
    Task<IReadOnlyList<DiagnosticRepairAction>> GetAvailableActionsAsync(
        DiagnosticsCheckResult item,
        CancellationToken cancellationToken = default);

    Task<DiagnosticRepairResult> ExecuteAsync(
        DiagnosticRepairRequest request,
        CancellationToken cancellationToken = default);
}
