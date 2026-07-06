using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ITaskManager
{
    Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken);

    Task PauseAsync(Guid taskId, CancellationToken cancellationToken);

    Task ResumeAsync(Guid taskId, CancellationToken cancellationToken);

    Task CancelAsync(Guid taskId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskInfo>> GetActiveTasksAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken);
}
