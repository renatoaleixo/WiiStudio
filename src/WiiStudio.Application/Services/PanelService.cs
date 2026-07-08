using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class PanelService : IPanelService
{
    public Task OpenRightPanelAsync(object viewModel, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task OpenBottomPanelAsync(object viewModel, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task ClosePanelAsync(string panelId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task TogglePanelAsync(string panelId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
