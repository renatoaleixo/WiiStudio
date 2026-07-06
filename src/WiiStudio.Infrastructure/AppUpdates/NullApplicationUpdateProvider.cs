using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.AppUpdates;

public sealed class NullApplicationUpdateProvider : IApplicationUpdateProvider
{
    public Task<ApplicationUpdateInfo?> CheckAsync(string currentVersion, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<ApplicationUpdateInfo?>(null);
    }

    public Task<ApplicationUpdatePackage?> DownloadAsync(ApplicationUpdateInfo update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<ApplicationUpdatePackage?>(null);
    }
}
