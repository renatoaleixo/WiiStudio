namespace WiiStudio.Application.DTOs;

public enum GameTdbUpdateStatus
{
    UpToDate = 1,
    Applied = 2,
    MirrorUnavailable = 3,
    InvalidChecksum = 4,
    Failed = 5
}
