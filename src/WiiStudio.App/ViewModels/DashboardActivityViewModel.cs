using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class DashboardActivityViewModel
{
    public DashboardActivityViewModel(DashboardActivity activity)
    {
        Title = activity.Title;
        Detail = activity.Detail;
        OccurredAt = activity.OccurredAt.ToLocalTime().ToString("dd/MM HH:mm");
    }

    public string Title { get; }

    public string Detail { get; }

    public string OccurredAt { get; }
}
