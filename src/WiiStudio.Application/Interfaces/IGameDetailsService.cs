using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameDetailsService
{
    Task<GameDetailsDto> GetAsync(string gameId, CancellationToken cancellationToken);

    Task RefreshAsync(string gameId, CancellationToken cancellationToken);
}
