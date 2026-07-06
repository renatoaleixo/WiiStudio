using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Notifications;

public sealed class JsonNotificationService : INotificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly NotificationStoreOptions _options;

    public JsonNotificationService(NotificationStoreOptions options)
    {
        _options = options;
    }

    public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        Validate(message);
        cancellationToken.ThrowIfCancellationRequested();

        var history = await LoadHistoryAsync(cancellationToken);
        var existingIndex = history.FindIndex(candidate => candidate.Id == message.Id);
        if (existingIndex >= 0)
        {
            history[existingIndex] = message;
        }
        else
        {
            history.Add(message);
        }

        await SaveHistoryAsync(ApplyHistoryLimit(history), cancellationToken);
    }

    public async Task DismissAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var history = await LoadHistoryAsync(cancellationToken);
        var index = history.FindIndex(candidate => candidate.Id == notificationId);
        if (index < 0)
        {
            return;
        }

        history[index] = history[index] with { IsDismissed = true };
        await SaveHistoryAsync(history, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationMessage>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        var history = await LoadHistoryAsync(cancellationToken);

        return history
            .OrderByDescending(notification => notification.CreatedAt)
            .ToArray();
    }

    public Task ClearHistoryAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return SaveHistoryAsync([], cancellationToken);
    }

    private async Task<List<NotificationMessage>> LoadHistoryAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_options.FilePath);
        var history = await JsonSerializer.DeserializeAsync<NotificationMessage[]>(stream, JsonOptions, cancellationToken);

        return (history ?? [])
            .Where(IsValid)
            .OrderBy(notification => notification.CreatedAt)
            .ToList();
    }

    private async Task SaveHistoryAsync(IReadOnlyList<NotificationMessage> history, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_options.FilePath);
        await JsonSerializer.SerializeAsync(stream, history, JsonOptions, cancellationToken);
    }

    private IReadOnlyList<NotificationMessage> ApplyHistoryLimit(List<NotificationMessage> history)
    {
        var limit = Math.Max(1, _options.HistoryLimit);

        return history
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(limit)
            .OrderBy(notification => notification.CreatedAt)
            .ToArray();
    }

    private static void Validate(NotificationMessage message)
    {
        if (message.Id == Guid.Empty)
        {
            throw new ArgumentException("Notification id must be provided.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(message.Title))
        {
            throw new ArgumentException("Notification title must be provided.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(message.Message))
        {
            throw new ArgumentException("Notification message must be provided.", nameof(message));
        }

        if (message.Progress is < 0 or > 1)
        {
            throw new ArgumentException("Notification progress must be between 0 and 1.", nameof(message));
        }
    }

    private static bool IsValid(NotificationMessage message)
    {
        return message.Id != Guid.Empty
            && !string.IsNullOrWhiteSpace(message.Title)
            && !string.IsNullOrWhiteSpace(message.Message)
            && message.Progress is null or (>= 0 and <= 1);
    }
}
