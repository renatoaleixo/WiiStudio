using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class EmulatorLaunchHistoryItemViewModel
{
    public EmulatorLaunchHistoryItemViewModel(EmulatorLaunchHistoryEntry entry, string profileName)
    {
        Result = entry.Result switch
        {
            "Started" => "Iniciado",
            "Failed" => "Falhou",
            "Blocked" => "Bloqueado",
            "Cancelled" => "Cancelado",
            _ => entry.Result
        };
        ProfileName = profileName;
        StartedAt = entry.StartedAt.ToLocalTime().ToString("g");
    }

    public string Result { get; }
    public string ProfileName { get; }
    public string StartedAt { get; }
}
