using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class GameDetailsActionItemViewModel
{
    public GameDetailsActionItemViewModel(GameDetailsActionAvailability action)
    {
        Action = action.Action;
        DisplayName = action.DisplayName;
        IsAvailable = action.IsAvailable;
        StatusText = action.IsAvailable ? "Disponível" : action.UnavailableReason ?? "Indisponível";
    }

    public GameDetailsAction Action { get; }

    public string DisplayName { get; }

    public bool IsAvailable { get; }

    public string StatusText { get; }
}
