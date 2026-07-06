using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface INotificationService
{
    Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken);

    Task DismissAsync(Guid notificationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationMessage>> GetHistoryAsync(CancellationToken cancellationToken);

    Task ClearHistoryAsync(CancellationToken cancellationToken);
}
