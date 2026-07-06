namespace WiiStudio.Application.DTOs;

public sealed record DashboardWidget(
    string Id,
    string Title,
    string Value,
    string Detail,
    DashboardState State = DashboardState.Ready);
