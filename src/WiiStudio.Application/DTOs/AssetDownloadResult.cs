namespace WiiStudio.Application.DTOs;

public sealed record AssetDownloadResult(
    IReadOnlyList<AssetDownloadItem> Items,
    DateTimeOffset CompletedAt)
{
    public bool HasFailures => Items.Any(item => item.Status is AssetDownloadStatus.Missing or AssetDownloadStatus.Failed);
}
