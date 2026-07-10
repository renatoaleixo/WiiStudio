using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameMetadataRepository
{
    Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken);

    Task UpsertAsync(GameMetadataDto metadata, CancellationToken cancellationToken);

    Task DeleteAsync(string gameId, CancellationToken cancellationToken);
}
