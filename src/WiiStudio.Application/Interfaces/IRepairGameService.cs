using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IRepairGameService
{
    Task<RepairPlan> AnalyzeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);

    Task<RepairResult> ExecuteAsync(RepairPlan plan, CancellationToken cancellationToken);
}
