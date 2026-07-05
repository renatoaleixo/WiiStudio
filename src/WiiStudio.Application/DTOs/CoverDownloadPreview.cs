namespace WiiStudio.Application.DTOs;

public sealed record CoverDownloadPreview(IReadOnlyList<CoverDownloadItem> Items)
{
    public int TotalItems => Items.Count;

    public bool HasMissingAssets => Items.Any(item => item.Source == CoverAssetSource.Missing);
}
