namespace WiiStudio.Application.DTOs;

public sealed record ExportResult(
    IReadOnlyList<ExportResultItem> Items,
    DateTimeOffset ExportedAt)
{
    public int TotalGames => Items.Count;

    public bool HasFailures => Items.Any(item => item.Status == ExportStatus.Failed);
}
