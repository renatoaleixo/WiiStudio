namespace WiiStudio.Application.DTOs;

public sealed record DockLayout(
    string Name,
    IReadOnlyList<DockPanelInfo> Panels,
    DockLayoutState State,
    DateTimeOffset UpdatedAt);
