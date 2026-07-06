using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IApplicationUpdateInstaller
{
    Task InstallAsync(ApplicationUpdatePackage package, CancellationToken cancellationToken);

    Task RollbackAsync(ApplicationUpdateState state, CancellationToken cancellationToken);
}
