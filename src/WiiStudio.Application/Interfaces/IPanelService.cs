namespace WiiStudio.Application.Interfaces;

public interface IPanelService
{
    Task OpenRightPanelAsync(object viewModel, CancellationToken cancellationToken = default);

    Task OpenBottomPanelAsync(object viewModel, CancellationToken cancellationToken = default);

    Task ClosePanelAsync(string panelId, CancellationToken cancellationToken = default);

    Task TogglePanelAsync(string panelId, CancellationToken cancellationToken = default);
}
