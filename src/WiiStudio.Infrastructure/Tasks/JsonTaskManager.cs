using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Tasks;

public sealed class JsonTaskManager : ITaskManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly HashSet<ManagedTaskStatus> ActiveStatuses =
    [
        ManagedTaskStatus.Queued,
        ManagedTaskStatus.Preparing,
        ManagedTaskStatus.Running,
        ManagedTaskStatus.Paused
    ];

    private readonly TaskManagerOptions _options;
    private readonly INotificationService _notificationService;

    public JsonTaskManager(TaskManagerOptions options, INotificationService notificationService)
    {
        _options = options;
        _notificationService = notificationService;
    }

    public async Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        var task = new TaskInfo(
            Guid.NewGuid(),
            request.Type,
            ManagedTaskStatus.Queued,
            request.Title.Trim(),
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            request.Priority,
            Progress: 0,
            now,
            now);

        var tasks = await LoadTasksAsync(cancellationToken);
        tasks.Add(task);
        await SaveTasksAsync(ApplyHistoryLimit(tasks), cancellationToken);
        await PublishNotificationAsync(task, NotificationType.Information, "Task queued.", cancellationToken);

        return task.Id;
    }

    public async Task PauseAsync(Guid taskId, CancellationToken cancellationToken)
    {
        await ChangeStatusAsync(taskId, ManagedTaskStatus.Paused, NotificationType.Information, "Task paused.", cancellationToken);
    }

    public async Task ResumeAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var tasks = await LoadTasksAsync(cancellationToken);
        var index = FindTaskIndex(tasks, taskId);
        var task = tasks[index];
        if (task.Status != ManagedTaskStatus.Paused)
        {
            throw new InvalidOperationException("Only paused tasks can be resumed.");
        }

        var updated = task with { Status = ManagedTaskStatus.Queued, UpdatedAt = DateTimeOffset.UtcNow };
        tasks[index] = updated;
        await SaveTasksAsync(tasks, cancellationToken);
        await PublishNotificationAsync(updated, NotificationType.Information, "Task resumed.", cancellationToken);
    }

    public async Task CancelAsync(Guid taskId, CancellationToken cancellationToken)
    {
        await ChangeStatusAsync(taskId, ManagedTaskStatus.Cancelled, NotificationType.Warning, "Task cancelled.", cancellationToken);
    }

    public async Task<IReadOnlyList<TaskInfo>> GetActiveTasksAsync(CancellationToken cancellationToken)
    {
        var tasks = await LoadTasksAsync(cancellationToken);

        return tasks
            .Where(task => ActiveStatuses.Contains(task.Status))
            .OrderByDescending(task => task.Priority)
            .ThenBy(task => task.CreatedAt)
            .ToArray();
    }

    public async Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        var tasks = await LoadTasksAsync(cancellationToken);

        return tasks
            .Where(task => !ActiveStatuses.Contains(task.Status))
            .OrderByDescending(task => task.FinishedAt ?? task.UpdatedAt)
            .ToArray();
    }

    private async Task ChangeStatusAsync(
        Guid taskId,
        ManagedTaskStatus status,
        NotificationType notificationType,
        string notificationMessage,
        CancellationToken cancellationToken)
    {
        var tasks = await LoadTasksAsync(cancellationToken);
        var index = FindTaskIndex(tasks, taskId);
        var task = tasks[index];
        EnsureCanTransition(task.Status, status);

        var now = DateTimeOffset.UtcNow;
        var updated = task with
        {
            Status = status,
            UpdatedAt = now,
            FinishedAt = status is ManagedTaskStatus.Completed or ManagedTaskStatus.Failed or ManagedTaskStatus.Cancelled ? now : task.FinishedAt
        };
        tasks[index] = updated;
        await SaveTasksAsync(ApplyHistoryLimit(tasks), cancellationToken);
        await PublishNotificationAsync(updated, notificationType, notificationMessage, cancellationToken);
    }

    private async Task<List<TaskInfo>> LoadTasksAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_options.FilePath);
        var tasks = await JsonSerializer.DeserializeAsync<TaskInfo[]>(stream, JsonOptions, cancellationToken);

        return (tasks ?? [])
            .Where(IsValid)
            .OrderBy(task => task.CreatedAt)
            .ToList();
    }

    private async Task SaveTasksAsync(IReadOnlyList<TaskInfo> tasks, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_options.FilePath);
        await JsonSerializer.SerializeAsync(stream, tasks, JsonOptions, cancellationToken);
    }

    private async Task PublishNotificationAsync(
        TaskInfo task,
        NotificationType type,
        string message,
        CancellationToken cancellationToken)
    {
        await _notificationService.PublishAsync(
            NotificationMessage.Create(
                type,
                task.Title,
                message,
                isPersistent: task.Status is ManagedTaskStatus.Failed or ManagedTaskStatus.Cancelled,
                groupKey: task.Id.ToString("N"),
                progress: task.Status == ManagedTaskStatus.Cancelled ? null : task.Progress),
            cancellationToken);
    }

    private IReadOnlyList<TaskInfo> ApplyHistoryLimit(List<TaskInfo> tasks)
    {
        var activeTasks = tasks.Where(task => ActiveStatuses.Contains(task.Status));
        var historyLimit = Math.Max(1, _options.HistoryLimit);
        var history = tasks
            .Where(task => !ActiveStatuses.Contains(task.Status))
            .OrderByDescending(task => task.FinishedAt ?? task.UpdatedAt)
            .Take(historyLimit);

        return activeTasks
            .Concat(history)
            .OrderBy(task => task.CreatedAt)
            .ToArray();
    }

    private static int FindTaskIndex(IReadOnlyList<TaskInfo> tasks, Guid taskId)
    {
        var index = tasks.ToList().FindIndex(task => task.Id == taskId);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Task '{taskId}' was not found.");
        }

        return index;
    }

    private static void EnsureCanTransition(ManagedTaskStatus current, ManagedTaskStatus next)
    {
        if (!ActiveStatuses.Contains(current))
        {
            throw new InvalidOperationException("Completed tasks cannot be changed.");
        }

        if (next == ManagedTaskStatus.Paused && current is ManagedTaskStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled tasks cannot be paused.");
        }
    }

    private static void Validate(TaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Task title must be provided.", nameof(request));
        }
    }

    private static bool IsValid(TaskInfo task)
    {
        return task.Id != Guid.Empty
            && !string.IsNullOrWhiteSpace(task.Title)
            && task.Progress is >= 0 and <= 1;
    }
}
