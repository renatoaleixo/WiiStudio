using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class CoverService : ICoverService
{
    private readonly ICoverCache _coverCache;

    public CoverService(ICoverCache coverCache)
    {
        _coverCache = coverCache;
    }

    public Task<Stream?> GetCoverAsync(string gameId, CancellationToken cancellationToken)
    {
        return GetCoverAsync(gameId, CoverAssetKind.FrontCover, cancellationToken);
    }

    public Task<Stream?> GetCoverAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        var normalizedGameId = GameIdValidation.Normalize(gameId);

        return _coverCache.OpenReadAsync(normalizedGameId, kind, cancellationToken);
    }
}
