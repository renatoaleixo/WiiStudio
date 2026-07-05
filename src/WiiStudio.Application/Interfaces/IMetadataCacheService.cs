using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IMetadataCacheService
{
    Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken);

    Task SaveAsync(GameMetadataDto metadata, CancellationToken cancellationToken);

    Task InvalidateAsync(string gameId, CancellationToken cancellationToken);

    Task<MetadataCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken);

    Task CleanupAsync(MetadataCleanupOptions options, CancellationToken cancellationToken);
}
