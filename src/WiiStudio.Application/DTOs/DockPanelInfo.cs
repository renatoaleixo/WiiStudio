namespace WiiStudio.Application.DTOs;

public sealed record DockPanelInfo(
    string Id,
    string Title,
    DockRegion Region,
    DockPanelState State,
    int Order,
    double Width = 300,
    double Height = 220);
