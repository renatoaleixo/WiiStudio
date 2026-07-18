using System.Globalization;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class GameDetailsHistoryItemViewModel
{
    public GameDetailsHistoryItemViewModel(GameDetailsHistoryItem item)
    {
        Operation = item.Operation;
        StatusText = item.Success ? "OK" : "Falha";
        Message = string.IsNullOrWhiteSpace(item.Message) ? "-" : item.Message;
        CreatedAt = item.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
    }

    public string Operation { get; }

    public string StatusText { get; }

    public string Message { get; }

    public string CreatedAt { get; }
}
