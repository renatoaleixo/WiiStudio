using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameTdbUpdateService
{
    Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken);

    Task<UpdateResult> UpdateAsync(GameTdbUpdateOptions options, CancellationToken cancellationToken);

    Task RollbackAsync(CancellationToken cancellationToken);
}
