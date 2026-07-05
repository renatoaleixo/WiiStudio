namespace WiiStudio.Application.DTOs;

public sealed record MetadataSyncPreview(IReadOnlyList<MetadataSyncItem> Items)
{
    public bool HasMissingItems => Items.Any(item => item.Status is MetadataSyncStatus.Missing or MetadataSyncStatus.ProviderUnavailable);
}
