namespace WiiStudio.Application.DTOs;

public sealed record CoverDownloadResult(
    IReadOnlyList<CoverDownloadItem> Items,
    DateTimeOffset DownloadedAt)
{
    public int TotalItems => Items.Count;

    public bool HasFailures => Items.Any(item => item.Source == CoverAssetSource.Missing);
}
