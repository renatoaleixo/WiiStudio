using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameLibraryRepository
{
    Task UpsertGamesAsync(IReadOnlyList<GameDto> games, CancellationToken cancellationToken);

    Task<IReadOnlyList<GameDto>> GetGamesAsync(CancellationToken cancellationToken);

    Task<GameDto?> GetByGameIdAsync(string gameId, CancellationToken cancellationToken);
}
