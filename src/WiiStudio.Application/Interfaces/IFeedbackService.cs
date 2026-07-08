using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IFeedbackService
{
    Task ShowAsync(FeedbackMessage message, CancellationToken cancellationToken = default);

    Task DismissAsync(string messageId, CancellationToken cancellationToken = default);
}
