using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Covers;

public sealed class NullCoverProvider : ICoverProvider
{
    public Task<Stream?> DownloadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<Stream?>(null);
    }
}
