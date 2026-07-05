namespace WiiStudio.Application.DTOs;

public sealed record ExportPreview(IReadOnlyList<ExportPreviewItem> Items)
{
    public int TotalGames => Items.Count;

    public bool HasConflicts => Items.Any(item => item.HasConflict);
}
