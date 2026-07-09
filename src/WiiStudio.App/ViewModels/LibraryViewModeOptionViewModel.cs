using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class LibraryViewModeOptionViewModel
{
    public LibraryViewModeOptionViewModel(LibraryViewMode mode, string displayName)
    {
        Mode = mode;
        DisplayName = displayName;
    }

    public LibraryViewMode Mode { get; }

    public string DisplayName { get; }
}
