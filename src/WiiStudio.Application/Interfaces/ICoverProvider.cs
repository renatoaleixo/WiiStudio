using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ICoverProvider
{
    Task<Stream?> DownloadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken);
}
