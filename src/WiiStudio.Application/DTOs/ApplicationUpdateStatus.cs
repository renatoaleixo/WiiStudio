namespace WiiStudio.Application.DTOs;

public enum ApplicationUpdateStatus
{
    Idle = 1,
    Checking = 2,
    Downloading = 3,
    Validating = 4,
    Installing = 5,
    RestartPending = 6,
    Completed = 7,
    Failed = 8,
    RolledBack = 9
}
