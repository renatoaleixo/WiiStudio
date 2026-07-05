using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ICoverDownloadService
{
    Task<CoverDownloadPreview> AnalyzeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);

    Task<CoverDownloadResult> DownloadAsync(CoverDownloadRequest request, CancellationToken cancellationToken);
}
