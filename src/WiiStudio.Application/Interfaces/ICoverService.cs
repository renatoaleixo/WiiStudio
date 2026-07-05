using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ICoverService
{
    Task<Stream?> GetCoverAsync(string gameId, CancellationToken cancellationToken);

    Task<Stream?> GetCoverAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken);
}
