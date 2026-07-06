using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record DashboardData(
    int TotalGames,
    IReadOnlyDictionary<FileFormat, int> GamesByFormat,
    long LibrarySizeInBytes,
    long DeviceFreeSpaceInBytes,
    int ConnectedDevices,
    int ActiveTasks,
    int ActiveConversions,
    int ActiveDownloads,
    int RecentNotifications,
    DateTimeOffset LoadedAt,
    IReadOnlyList<DashboardWidget> Widgets,
    IReadOnlyList<DashboardActivity> Activities,
    IReadOnlyList<DashboardQuickAction> QuickActions);
