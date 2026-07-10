using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameAssetRepository
{
    Task UpsertAsync(AssetInfo asset, CancellationToken cancellationToken);

    Task DeleteAsync(AssetKey key, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetInfo>> GetByGameIdAsync(string gameId, CancellationToken cancellationToken);
}
