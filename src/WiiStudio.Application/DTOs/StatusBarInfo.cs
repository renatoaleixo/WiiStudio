namespace WiiStudio.Application.DTOs;

public sealed record StatusBarInfo(
    string Message,
    string ActiveDevice,
    string FileSystem,
    string FreeSpace,
    int SelectedGames,
    int ActiveTasks,
    int PendingNotifications,
    string MemoryUsage,
    string ConnectionStatus,
    string Version,
    StatusBarState State,
    DateTimeOffset RefreshedAt);
