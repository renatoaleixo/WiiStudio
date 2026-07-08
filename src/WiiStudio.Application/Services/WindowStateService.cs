using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class WindowStateService : IWindowStateService
{
    public Task<WindowStateSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new WindowStateSettings(1280, 800, null, null, IsMaximized: false));
    }

    public Task SaveAsync(WindowStateSettings settings, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public bool IsValidPosition(WindowStateSettings settings)
    {
        return settings.Width >= 1024 && settings.Height >= 640;
    }
}
