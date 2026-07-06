using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameTdbUpdateStateStore
{
    Task<GameTdbUpdateState> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(GameTdbUpdateState state, CancellationToken cancellationToken);

    Task RollbackAsync(CancellationToken cancellationToken);
}
