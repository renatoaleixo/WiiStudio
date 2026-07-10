using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ITaskManager
{
    Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken);

    Task UpdateProgressAsync(Guid taskId, double progress, ManagedTaskStatus status, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotSupportedException("Task progress updates are not supported by this task manager."));
    }

    Task CompleteAsync(Guid taskId, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotSupportedException("Task completion is not supported by this task manager."));
    }

    Task FailAsync(Guid taskId, string errorMessage, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotSupportedException("Task failure is not supported by this task manager."));
    }

    Task PauseAsync(Guid taskId, CancellationToken cancellationToken);

    Task ResumeAsync(Guid taskId, CancellationToken cancellationToken);

    Task CancelAsync(Guid taskId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskInfo>> GetActiveTasksAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken);
}
