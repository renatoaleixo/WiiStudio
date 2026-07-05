using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameMetadataProvider
{
    Task<GameMetadataDto?> GetByGameIdAsync(string gameId, CancellationToken cancellationToken);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}
