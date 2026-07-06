using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDeviceManagerService _deviceManagerService;
    private readonly IGameLibraryService _gameLibraryService;
    private readonly INotificationService _notificationService;
    private readonly ITaskManager _taskManager;
    private DashboardData? _currentData;

    public DashboardService(
        IGameLibraryService gameLibraryService,
        IDeviceManagerService deviceManagerService,
        ITaskManager taskManager,
        INotificationService notificationService)
    {
        _gameLibraryService = gameLibraryService;
        _deviceManagerService = deviceManagerService;
        _taskManager = taskManager;
        _notificationService = notificationService;
    }

    public async Task<DashboardData> LoadAsync(CancellationToken cancellationToken)
    {
        if (_currentData is not null)
        {
            return _currentData;
        }

        return await RefreshAsync(cancellationToken);
    }

    public async Task<DashboardData> RefreshAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var games = await _gameLibraryService.GetGamesAsync(null, cancellationToken);
        var devices = await _deviceManagerService.GetDevicesAsync(cancellationToken);
        var activeTasks = await _taskManager.GetActiveTasksAsync(cancellationToken);
        var notifications = await _notificationService.GetHistoryAsync(cancellationToken);

        var gamesByFormat = games
            .GroupBy(game => game.Format)
            .ToDictionary(group => group.Key, group => group.Count());
        var librarySize = games.Sum(game => game.SizeInBytes);
        var freeSpace = devices.Sum(device => device.FreeSpace);
        var activeConversions = activeTasks.Count(task => task.Type == ManagedTaskType.Conversion);
        var activeDownloads = activeTasks.Count(task => task.Type == ManagedTaskType.Download);
        var recentNotifications = notifications.Count(notification => !notification.IsDismissed);
        var loadedAt = DateTimeOffset.UtcNow;

        _currentData = new DashboardData(
            games.Count,
            gamesByFormat,
            librarySize,
            freeSpace,
            devices.Count,
            activeTasks.Count,
            activeConversions,
            activeDownloads,
            recentNotifications,
            loadedAt,
            CreateWidgets(games.Count, librarySize, devices.Count, freeSpace, activeTasks.Count, activeConversions, activeDownloads, recentNotifications),
            CreateActivities(activeTasks, notifications),
            CreateQuickActions());

        return _currentData;
    }

    private static IReadOnlyList<DashboardWidget> CreateWidgets(
        int totalGames,
        long librarySize,
        int connectedDevices,
        long freeSpace,
        int activeTasks,
        int activeConversions,
        int activeDownloads,
        int recentNotifications)
    {
        return
        [
            new DashboardWidget("library", "Biblioteca", totalGames.ToString(), $"{FormatSize(librarySize)} ocupados"),
            new DashboardWidget("devices", "Dispositivos", connectedDevices.ToString(), $"{FormatSize(freeSpace)} livres"),
            new DashboardWidget("tasks", "Tarefas", activeTasks.ToString(), $"{activeConversions} conversões, {activeDownloads} downloads"),
            new DashboardWidget("alerts", "Alertas", recentNotifications.ToString(), "notificações recentes")
        ];
    }

    private static IReadOnlyList<DashboardActivity> CreateActivities(
        IReadOnlyList<TaskInfo> activeTasks,
        IReadOnlyList<NotificationMessage> notifications)
    {
        var taskActivities = activeTasks.Select(task => new DashboardActivity(
            task.Title,
            $"{task.Type} · {task.Status} · {task.Progress:P0}",
            task.UpdatedAt));

        var notificationActivities = notifications
            .Where(notification => !notification.IsDismissed)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(5)
            .Select(notification => new DashboardActivity(notification.Title, notification.Message, notification.CreatedAt));

        return taskActivities
            .Concat(notificationActivities)
            .OrderByDescending(activity => activity.OccurredAt)
            .Take(8)
            .ToArray();
    }

    private static IReadOnlyList<DashboardQuickAction> CreateQuickActions()
    {
        return
        [
            new DashboardQuickAction("scan-library", "Atualizar biblioteca", "Recarregar jogos e metadados."),
            new DashboardQuickAction("detect-devices", "Detectar dispositivos", "Procurar HDs, SSDs e pendrives."),
            new DashboardQuickAction("import-games", "Importar jogos", "Abrir fluxo de importação."),
            new DashboardQuickAction("convert-games", "Converter jogos", "Preparar conversões em lote.")
        ];
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
