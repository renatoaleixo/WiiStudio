using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IBatchCoverDownloadService
{
    Task<CoverBatchDownloadSummary> DownloadAsync(
        IReadOnlyList<string> gameIds,
        IReadOnlyList<CoverAssetKind> assetKinds,
        bool forceRefresh,
        IProgress<CoverBatchProgress>? progress,
        CancellationToken cancellationToken = default);
}
