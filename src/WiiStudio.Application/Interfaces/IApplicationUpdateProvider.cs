using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IApplicationUpdateProvider
{
    Task<ApplicationUpdateInfo?> CheckAsync(string currentVersion, CancellationToken cancellationToken);

    Task<ApplicationUpdatePackage?> DownloadAsync(ApplicationUpdateInfo update, CancellationToken cancellationToken);
}
