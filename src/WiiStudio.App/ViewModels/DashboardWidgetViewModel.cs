using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class DashboardWidgetViewModel
{
    public DashboardWidgetViewModel(DashboardWidget widget)
    {
        Id = widget.Id;
        Title = widget.Title;
        Value = widget.Value;
        Detail = widget.Detail;
        State = widget.State.ToString();
    }

    public string Id { get; }

    public string Title { get; }

    public string Value { get; }

    public string Detail { get; }

    public string State { get; }
}
