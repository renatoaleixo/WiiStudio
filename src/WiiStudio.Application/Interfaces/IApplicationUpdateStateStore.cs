using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IApplicationUpdateStateStore
{
    Task<ApplicationUpdateState> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(ApplicationUpdateState state, CancellationToken cancellationToken);
}
