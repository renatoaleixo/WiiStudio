using System.Globalization;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class TaskItemViewModel
{
    public TaskItemViewModel(TaskInfo task)
    {
        Id = task.Id;
        Title = task.Title;
        Description = string.IsNullOrWhiteSpace(task.Description) ? task.Type.ToString() : task.Description;
        Status = ToDisplayText(task.Status);
        Priority = ToDisplayText(task.Priority);
        Progress = Math.Clamp(task.Progress, 0, 1) * 100;
        UpdatedAt = task.UpdatedAt.ToLocalTime().ToString("dd/MM HH:mm", CultureInfo.CurrentCulture);
        ErrorMessage = task.ErrorMessage;
        HasError = !string.IsNullOrWhiteSpace(task.ErrorMessage);
        IsActive = task.Status is ManagedTaskStatus.Queued or ManagedTaskStatus.Preparing or ManagedTaskStatus.Running or ManagedTaskStatus.Paused;
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Description { get; }

    public string Status { get; }

    public string Priority { get; }

    public double Progress { get; }

    public string UpdatedAt { get; }

    public string? ErrorMessage { get; }

    public bool HasError { get; }

    public bool IsActive { get; }

    private static string ToDisplayText(ManagedTaskStatus status)
    {
        return status switch
        {
            ManagedTaskStatus.Queued => "Na fila",
            ManagedTaskStatus.Preparing => "Preparando",
            ManagedTaskStatus.Running => "Executando",
            ManagedTaskStatus.Paused => "Pausada",
            ManagedTaskStatus.Completed => "Concluida",
            ManagedTaskStatus.Failed => "Falhou",
            ManagedTaskStatus.Cancelled => "Cancelada",
            _ => status.ToString()
        };
    }

    private static string ToDisplayText(ManagedTaskPriority priority)
    {
        return priority switch
        {
            ManagedTaskPriority.Low => "Baixa",
            ManagedTaskPriority.Normal => "Normal",
            ManagedTaskPriority.High => "Alta",
            _ => priority.ToString()
        };
    }
}
