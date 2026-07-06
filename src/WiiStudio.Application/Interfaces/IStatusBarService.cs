using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IStatusBarService
{
    Task<StatusBarInfo> GetAsync(CancellationToken cancellationToken);

    Task<StatusBarInfo> RefreshAsync(CancellationToken cancellationToken);
}
