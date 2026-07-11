using System.Globalization;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class NotificationItemViewModel
{
    public NotificationItemViewModel(NotificationMessage notification)
    {
        Id = notification.Id;
        Title = notification.Title;
        Message = notification.Message;
        Type = ToDisplayText(notification.Type);
        CreatedAt = notification.CreatedAt.ToLocalTime().ToString("dd/MM HH:mm", CultureInfo.CurrentCulture);
        IsDismissed = notification.IsDismissed;
        IsPersistent = notification.IsPersistent;
        HasProgress = notification.Progress is not null;
        Progress = Math.Clamp(notification.Progress ?? 0, 0, 1) * 100;
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Message { get; }

    public string Type { get; }

    public string CreatedAt { get; }

    public bool IsDismissed { get; }

    public bool IsPersistent { get; }

    public bool HasProgress { get; }

    public double Progress { get; }

    private static string ToDisplayText(NotificationType type)
    {
        return type switch
        {
            NotificationType.Information => "Info",
            NotificationType.Success => "Sucesso",
            NotificationType.Warning => "Aviso",
            NotificationType.Error => "Erro",
            NotificationType.Progress => "Progresso",
            NotificationType.Persistent => "Persistente",
            _ => type.ToString()
        };
    }
}
