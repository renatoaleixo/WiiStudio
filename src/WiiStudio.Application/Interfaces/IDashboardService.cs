using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardData> LoadAsync(CancellationToken cancellationToken);

    Task<DashboardData> RefreshAsync(CancellationToken cancellationToken);
}
