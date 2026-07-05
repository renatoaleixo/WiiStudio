using WiiStudio.Application.Commands;

namespace WiiStudio.Application.Interfaces;

public interface ICoverDownloadService
{
    Task DownloadAsync(DownloadCoverCommand command, CancellationToken cancellationToken);
}
