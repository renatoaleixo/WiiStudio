namespace WiiStudio.Application.DTOs;

public enum ShellRegion
{
    Header,
    MainMenu,
    Sidebar,
    Workspace,
    RightPanel,
    StatusBar,
    NotificationHost,
    DialogHost,
    OverlayHost
}

public enum WorkspaceState
{
    Loading,
    Ready,
    Empty,
    Error,
    NotFound,
    Unauthorized
}

public enum PanelKind
{
    Left,
    Right,
    Bottom,
    Floating,
    Overlay
}

public enum PanelShellState
{
    Closed,
    Opening,
    Open,
    Closing,
    Collapsed,
    Loading,
    Error,
    Disabled
}

public sealed record AppShellRegion(
    ShellRegion Region,
    string Name,
    bool IsVisible,
    bool IsOverlay = false);

public sealed record WorkspaceDescriptor(
    string Route,
    WorkspaceState State,
    object? CurrentViewModel,
    string? ErrorMessageKey = null);

public sealed record ShellPanelDescriptor(
    string Id,
    PanelKind Kind,
    PanelShellState State,
    string TitleKey,
    object? ContentViewModel = null);
