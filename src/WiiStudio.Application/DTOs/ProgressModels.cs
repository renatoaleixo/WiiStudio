namespace WiiStudio.Application.DTOs;

public enum OperationStatus
{
    Idle,
    Starting,
    Running,
    Paused,
    Cancelling,
    Cancelled,
    Completed,
    Failed,
    Partial,
    Indeterminate
}

public sealed record ProgressState(
    string TaskId,
    string TitleKey,
    string? CurrentOperationKey,
    double? Percentage,
    int? CurrentItem,
    int? TotalItems,
    bool IsIndeterminate,
    bool CanCancel,
    OperationStatus Status);
