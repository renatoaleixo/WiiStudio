using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ICoverAssetBulkActionService
{
    Task<CoverAssetBulkActionResult> ExecuteAsync(
        CoverAssetBulkActionRequest request,
        IProgress<CoverAssetBulkActionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
