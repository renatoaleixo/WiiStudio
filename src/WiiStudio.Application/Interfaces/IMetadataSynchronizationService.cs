using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IMetadataSynchronizationService
{
    Task<MetadataSyncPreview> AnalyzeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);

    Task<MetadataSyncResult> SynchronizeAsync(MetadataSyncRequest request, CancellationToken cancellationToken);
}
