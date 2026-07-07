using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class ThemeOptionViewModel
{
    public ThemeOptionViewModel(ThemeInfo theme)
    {
        Id = theme.Id;
        Name = theme.Name;
        Kind = theme.Kind.ToString();
        IsBuiltIn = theme.IsBuiltIn;
        IsActive = theme.IsActive;
    }

    public string Id { get; }

    public string Name { get; }

    public string Kind { get; }

    public bool IsBuiltIn { get; }

    public bool IsActive { get; }
}
