namespace WiiStudio.Application.DTOs;

public sealed record MetadataSyncResult(
    IReadOnlyList<MetadataSyncItem> Items,
    DateTimeOffset CompletedAt)
{
    public bool HasFailures => Items.Any(item => item.Status is MetadataSyncStatus.Failed or MetadataSyncStatus.ProviderUnavailable);

    public int UpdatedCount => Items.Count(item => item.Status == MetadataSyncStatus.Updated);
}
