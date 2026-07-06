namespace WiiStudio.Application.DTOs;

public enum ManagedTaskStatus
{
    Queued = 1,
    Preparing = 2,
    Running = 3,
    Paused = 4,
    Completed = 5,
    Failed = 6,
    Cancelled = 7
}
