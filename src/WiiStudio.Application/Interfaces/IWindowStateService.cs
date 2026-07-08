using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IWindowStateService
{
    Task<WindowStateSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(WindowStateSettings settings, CancellationToken cancellationToken = default);

    bool IsValidPosition(WindowStateSettings settings);
}
