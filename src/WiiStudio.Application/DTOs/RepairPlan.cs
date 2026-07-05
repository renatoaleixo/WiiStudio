namespace WiiStudio.Application.DTOs;

public sealed record RepairPlan(
    IReadOnlyList<RepairPlanItem> Items,
    DateTimeOffset PlannedAt)
{
    public int TotalGames => Items.Count;

    public bool HasExecutableActions => Items.Any(item => item.Action == RepairAction.RevalidateWithWiimms);
}
