using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class DashboardQuickActionViewModel
{
    public DashboardQuickActionViewModel(DashboardQuickAction action)
    {
        Id = action.Id;
        Title = action.Title;
        Description = action.Description;
    }

    public string Id { get; }

    public string Title { get; }

    public string Description { get; }
}
