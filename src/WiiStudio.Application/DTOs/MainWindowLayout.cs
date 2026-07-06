namespace WiiStudio.Application.DTOs;

public sealed record MainWindowLayout(
    ShellModuleId ActiveModuleId = ShellModuleId.Dashboard,
    bool IsSidePanelVisible = true,
    double NavigationWidth = 232,
    double SidePanelWidth = 300);
