namespace WiiStudio.Application.DTOs;

public sealed record RepairResult(
    IReadOnlyList<RepairResultItem> Items,
    DateTimeOffset RepairedAt)
{
    public int TotalGames => Items.Count;

    public bool HasFailures => Items.Any(item => item.Status == RepairStatus.Failed);
}
