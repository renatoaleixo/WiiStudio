using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IMainWindowService
{
    MainWindowLayout CurrentLayout { get; }

    IReadOnlyList<ShellModule> Modules { get; }

    Task<MainWindowSnapshot> InitializeAsync(CancellationToken cancellationToken);

    Task<MainWindowLayout> RestoreLayoutAsync(CancellationToken cancellationToken);

    Task SaveLayoutAsync(CancellationToken cancellationToken);

    Task SelectModuleAsync(ShellModuleId moduleId, CancellationToken cancellationToken);

    Task SetSidePanelVisibilityAsync(bool isVisible, CancellationToken cancellationToken);
}
