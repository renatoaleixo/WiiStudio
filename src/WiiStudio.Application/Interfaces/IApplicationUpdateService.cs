using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IApplicationUpdateService
{
    Task<ApplicationUpdateInfo> CheckAsync(CancellationToken cancellationToken);

    Task DownloadAsync(ApplicationUpdateInfo update, CancellationToken cancellationToken);

    Task InstallAsync(CancellationToken cancellationToken);

    Task RollbackAsync(CancellationToken cancellationToken);
}
