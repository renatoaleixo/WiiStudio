using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.AppUpdates;

public sealed class NullApplicationUpdateInstaller : IApplicationUpdateInstaller
{
    public Task InstallAsync(ApplicationUpdatePackage package, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }

    public Task RollbackAsync(ApplicationUpdateState state, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }
}
