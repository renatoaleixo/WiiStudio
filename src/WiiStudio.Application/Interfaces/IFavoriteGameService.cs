using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IFavoriteGameService
{
    Task AddAsync(string gameId, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);

    Task RemoveAsync(string gameId, CancellationToken cancellationToken);

    Task<bool> IsFavoriteAsync(string gameId, CancellationToken cancellationToken);

    Task<IReadOnlyList<GameDto>> GetFavoritesAsync(CancellationToken cancellationToken);
}
