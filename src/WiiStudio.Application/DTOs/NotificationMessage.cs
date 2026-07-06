namespace WiiStudio.Application.DTOs;

public sealed record NotificationMessage(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    DateTimeOffset CreatedAt,
    bool IsPersistent = false,
    bool IsDismissed = false,
    string? GroupKey = null,
    double? Progress = null,
    IReadOnlyList<NotificationAction>? Actions = null)
{
    public static NotificationMessage Create(
        NotificationType type,
        string title,
        string message,
        bool isPersistent = false,
        string? groupKey = null,
        double? progress = null,
        IReadOnlyList<NotificationAction>? actions = null)
    {
        return new NotificationMessage(
            Guid.NewGuid(),
            type,
            title,
            message,
            DateTimeOffset.UtcNow,
            isPersistent,
            IsDismissed: false,
            groupKey,
            progress,
            actions);
    }
}
