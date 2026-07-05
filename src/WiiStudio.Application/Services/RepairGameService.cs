using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class RepairGameService : IRepairGameService
{
    private readonly IVerifyGameService _verifyGameService;

    public RepairGameService(IVerifyGameService verifyGameService)
    {
        _verifyGameService = verifyGameService;
    }

    public async Task<RepairPlan> AnalyzeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        cancellationToken.ThrowIfCancellationRequested();

        var verificationReport = await _verifyGameService.VerifyAsync(gameIds, cancellationToken);
        var items = verificationReport.Results
            .Select(CreatePlanItem)
            .ToArray();

        return new RepairPlan(items, DateTimeOffset.UtcNow);
    }

    public async Task<RepairResult> ExecuteAsync(RepairPlan plan, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        var results = new List<RepairResultItem>();
        foreach (var item in plan.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await ExecuteItemAsync(item, cancellationToken));
        }

        return new RepairResult(results, DateTimeOffset.UtcNow);
    }

    private async Task<RepairResultItem> ExecuteItemAsync(RepairPlanItem item, CancellationToken cancellationToken)
    {
        return item.Action switch
        {
            RepairAction.NoAction => new RepairResultItem(item.GameId, item.Action, RepairStatus.Skipped, "No repair action was required."),
            RepairAction.ReimportRequired => new RepairResultItem(item.GameId, item.Action, RepairStatus.RequiresUserAction, "The game must be reimported."),
            RepairAction.RevalidateWithWiimms => await RevalidateAsync(item.GameId, cancellationToken),
            _ => new RepairResultItem(item.GameId, item.Action, RepairStatus.Failed, "Unknown repair action.")
        };
    }

    private async Task<RepairResultItem> RevalidateAsync(string gameId, CancellationToken cancellationToken)
    {
        var report = await _verifyGameService.VerifyAsync([gameId], cancellationToken);
        var result = report.Results.Single();

        return result.Status == VerificationStatus.Healthy
            ? new RepairResultItem(gameId, RepairAction.RevalidateWithWiimms, RepairStatus.Completed, "Game was revalidated successfully.")
            : new RepairResultItem(gameId, RepairAction.RevalidateWithWiimms, RepairStatus.Failed, result.Message);
    }

    private static RepairPlanItem CreatePlanItem(VerificationResultDto verificationResult)
    {
        return verificationResult.Status switch
        {
            VerificationStatus.Healthy => new RepairPlanItem(
                verificationResult.GameId,
                RepairAction.NoAction,
                verificationResult.Status,
                "Game is already healthy.",
                "None."),
            VerificationStatus.Warning => new RepairPlanItem(
                verificationResult.GameId,
                RepairAction.RevalidateWithWiimms,
                verificationResult.Status,
                verificationResult.Message,
                "Low. The game will only be checked again."),
            VerificationStatus.Corrupted => new RepairPlanItem(
                verificationResult.GameId,
                RepairAction.ReimportRequired,
                verificationResult.Status,
                verificationResult.Message,
                "High. Automatic repair is not attempted for corrupted media."),
            _ => new RepairPlanItem(
                verificationResult.GameId,
                RepairAction.ReimportRequired,
                verificationResult.Status,
                verificationResult.Message,
                "Medium. The current format or file state is not automatically repairable.")
        };
    }
}
