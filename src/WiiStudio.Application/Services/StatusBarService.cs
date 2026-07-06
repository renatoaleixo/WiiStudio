using System.Reflection;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class StatusBarService : IStatusBarService
{
    private readonly IDeviceManagerService _deviceManagerService;
    private readonly INotificationService _notificationService;
    private readonly ITaskManager _taskManager;
    private StatusBarInfo? _currentInfo;

    public StatusBarService(
        IDeviceManagerService deviceManagerService,
        ITaskManager taskManager,
        INotificationService notificationService)
    {
        _deviceManagerService = deviceManagerService;
        _taskManager = taskManager;
        _notificationService = notificationService;
    }

    public async Task<StatusBarInfo> GetAsync(CancellationToken cancellationToken)
    {
        return _currentInfo ?? await RefreshAsync(cancellationToken);
    }

    public async Task<StatusBarInfo> RefreshAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var devices = await _deviceManagerService.GetDevicesAsync(cancellationToken);
        var activeDevice = devices.FirstOrDefault(device => device.Status == DeviceStatus.Available)
            ?? devices.FirstOrDefault();
        var activeTasks = await _taskManager.GetActiveTasksAsync(cancellationToken);
        var notifications = await _notificationService.GetHistoryAsync(cancellationToken);
        var pendingNotifications = notifications.Count(notification => !notification.IsDismissed);
        var state = ResolveState(activeDevice, activeTasks.Count, pendingNotifications);

        _currentInfo = new StatusBarInfo(
            Message: state == StatusBarState.Warning ? "Atenção necessária." : "Pronto",
            ActiveDevice: activeDevice?.Name ?? "Nenhum dispositivo",
            FileSystem: activeDevice?.FileSystem.ToString() ?? "-",
            FreeSpace: activeDevice is null ? "-" : FormatSize(activeDevice.FreeSpace),
            SelectedGames: 0,
            ActiveTasks: activeTasks.Count,
            PendingNotifications: pendingNotifications,
            MemoryUsage: FormatSize(GC.GetTotalMemory(forceFullCollection: false)),
            ConnectionStatus: "Local",
            Version: GetVersion(),
            state,
            DateTimeOffset.UtcNow);

        return _currentInfo;
    }

    private static StatusBarState ResolveState(DeviceInfo? activeDevice, int activeTasks, int pendingNotifications)
    {
        if (activeDevice?.Status is DeviceStatus.Incompatible or DeviceStatus.ReadOnly or DeviceStatus.Unmounted)
        {
            return StatusBarState.Warning;
        }

        if (pendingNotifications > 0)
        {
            return StatusBarState.Warning;
        }

        return activeTasks > 0 ? StatusBarState.Updating : StatusBarState.Ready;
    }

    private static string GetVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(StatusBarService).Assembly;
        return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    }

    private static string FormatSize(long sizeInBytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = (double)sizeInBytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}
