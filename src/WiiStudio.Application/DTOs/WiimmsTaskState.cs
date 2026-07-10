namespace WiiStudio.Application.DTOs;

public enum WiimmsTaskState
{
    Pending,
    Starting,
    Running,
    Indeterminate,
    Cancelling,
    Completed,
    Failed,
    Cancelled,
    TimedOut
}
