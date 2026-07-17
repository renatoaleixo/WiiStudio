using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class DiagnosticsRepairService : IDiagnosticsRepairService
{
    public const string InstallPolkitPolicyAction = "repair.install-polkit-policy";
    public const string ValidateExternalToolsAction = "repair.validate-external-tools";
    public const string UpdateGameTdbAction = "repair.update-gametdb";

    private static readonly DiagnosticRepairAction InstallPolkitPolicy = new(
        InstallPolkitPolicyAction,
        "Instalar policy Polkit",
        "Instala a policy segura do WiiStudio para operações USB autorizadas.",
        RequiresConfirmation: true,
        IsDestructive: false,
        DiagnosticRepairRiskLevel.Elevated);

    private static readonly DiagnosticRepairAction ValidateExternalTools = new(
        ValidateExternalToolsAction,
        "Validar ferramentas externas",
        "Verifica wit, wwt e demais ferramentas configuradas.",
        RequiresConfirmation: false,
        IsDestructive: false,
        DiagnosticRepairRiskLevel.Safe);

    private static readonly DiagnosticRepairAction UpdateGameTdb = new(
        UpdateGameTdbAction,
        "Atualizar GameTDB",
        "Baixa e valida a base local de metadados GameTDB.",
        RequiresConfirmation: false,
        IsDestructive: false,
        DiagnosticRepairRiskLevel.Safe);

    private readonly IPolkitPolicyInstaller _polkitPolicyInstaller;
    private readonly IExternalToolManager _externalToolManager;
    private readonly IGameTdbUpdateService _gameTdbUpdateService;
    private readonly IDiagnosticsService _diagnosticsService;
    private readonly ITaskManager _taskManager;
    private readonly INotificationService _notificationService;
    private readonly IDiagnosticsRepairHistoryStore _historyStore;

    public DiagnosticsRepairService(
        IPolkitPolicyInstaller polkitPolicyInstaller,
        IExternalToolManager externalToolManager,
        IGameTdbUpdateService gameTdbUpdateService,
        IDiagnosticsService diagnosticsService,
        ITaskManager taskManager,
        INotificationService notificationService,
        IDiagnosticsRepairHistoryStore historyStore)
    {
        _polkitPolicyInstaller = polkitPolicyInstaller;
        _externalToolManager = externalToolManager;
        _gameTdbUpdateService = gameTdbUpdateService;
        _diagnosticsService = diagnosticsService;
        _taskManager = taskManager;
        _notificationService = notificationService;
        _historyStore = historyStore;
    }

    public Task<IReadOnlyList<DiagnosticRepairAction>> GetAvailableActionsAsync(
        DiagnosticsCheckResult item,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        cancellationToken.ThrowIfCancellationRequested();

        if (item.Status == DiagnosticsStatus.Healthy)
        {
            return Task.FromResult<IReadOnlyList<DiagnosticRepairAction>>([]);
        }

        var name = item.Name.ToLowerInvariant();
        var actions = new List<DiagnosticRepairAction>();
        if (name.Contains("polkit", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add(InstallPolkitPolicy);
        }

        if (name.Contains("wit", StringComparison.OrdinalIgnoreCase)
            || name.Contains("wwt", StringComparison.OrdinalIgnoreCase)
            || name.Contains("external-tool", StringComparison.OrdinalIgnoreCase)
            || name.Contains("wiimms", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add(ValidateExternalTools);
        }

        if (name.Contains("metadata", StringComparison.OrdinalIgnoreCase)
            || name.Contains("gametdb", StringComparison.OrdinalIgnoreCase))
        {
            actions.Add(UpdateGameTdb);
        }

        return Task.FromResult<IReadOnlyList<DiagnosticRepairAction>>(actions);
    }

    public Task<IReadOnlyList<DiagnosticRepairHistoryEntry>> GetHistoryAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return _historyStore.GetRecentAsync(limit, cancellationToken);
    }

    public async Task<DiagnosticRepairResult> ExecuteAsync(
        DiagnosticRepairRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var action = GetCatalogAction(request.ActionId);
        if (action.RequiresConfirmation && !request.UserConfirmed)
        {
            return new DiagnosticRepairResult(request.ActionId, Success: false, "Confirmação obrigatória para executar este reparo.");
        }

        var taskId = await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Repair, action.Title, action.Description),
            cancellationToken);
        var startedAt = DateTimeOffset.UtcNow;

        try
        {
            await _taskManager.UpdateProgressAsync(taskId, 0.25, ManagedTaskStatus.Running, cancellationToken);
            var message = await ExecuteCatalogActionAsync(action.ActionId, cancellationToken);
            await _taskManager.UpdateProgressAsync(taskId, 0.8, ManagedTaskStatus.Running, cancellationToken);
            var report = await _diagnosticsService.RunAsync(cancellationToken);
            await _taskManager.CompleteAsync(taskId, cancellationToken);
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Success, "Reparo concluído", message, groupKey: request.ActionId),
                cancellationToken);
            await RecordHistoryAsync(action, success: true, message, startedAt, request.CheckName, cancellationToken);
            return new DiagnosticRepairResult(request.ActionId, Success: true, message, report);
        }
        catch (OperationCanceledException exception)
        {
            await _taskManager.CancelAsync(taskId, CancellationToken.None);
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Warning, "Reparo cancelado", "Correção cancelada pelo usuário.", groupKey: request.ActionId),
                CancellationToken.None);
            await RecordHistoryAsync(action, success: false, exception.Message, startedAt, request.CheckName, CancellationToken.None);
            return new DiagnosticRepairResult(request.ActionId, Success: false, "Correção cancelada pelo usuário.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await _taskManager.FailAsync(taskId, exception.Message, CancellationToken.None);
            await _notificationService.PublishAsync(
                NotificationMessage.Create(NotificationType.Error, "Falha no reparo", exception.Message, isPersistent: true, groupKey: request.ActionId),
                CancellationToken.None);
            await RecordHistoryAsync(action, success: false, exception.Message, startedAt, request.CheckName, CancellationToken.None);
            return new DiagnosticRepairResult(request.ActionId, Success: false, exception.Message);
        }
    }

    private Task RecordHistoryAsync(
        DiagnosticRepairAction action,
        bool success,
        string message,
        DateTimeOffset startedAt,
        string? checkName,
        CancellationToken cancellationToken)
    {
        var entry = new DiagnosticRepairHistoryEntry(
            Guid.NewGuid(),
            action.ActionId,
            action.Title,
            success,
            message,
            startedAt,
            DateTimeOffset.UtcNow,
            string.IsNullOrWhiteSpace(checkName) ? null : checkName.Trim());

        return _historyStore.AppendAsync(entry, cancellationToken);
    }

    private async Task<string> ExecuteCatalogActionAsync(string actionId, CancellationToken cancellationToken)
    {
        return actionId switch
        {
            InstallPolkitPolicyAction => CreatePolkitInstallMessage(await _polkitPolicyInstaller.InstallAsync(cancellationToken)),
            ValidateExternalToolsAction => $"Ferramentas verificadas: {(await _externalToolManager.CheckAllAsync(cancellationToken)).Count}.",
            UpdateGameTdbAction => (await _gameTdbUpdateService.UpdateAsync(new GameTdbUpdateOptions(Force: true), cancellationToken)).Message,
            _ => throw new InvalidOperationException($"Ação de reparo desconhecida: {actionId}.")
        };
    }

    private static string CreatePolkitInstallMessage(PolkitPolicyInstallResult result)
    {
        if (result.Success)
        {
            return result.MessageKey ?? result.State.ToString();
        }

        var message = result.MessageKey ?? "Falha ao instalar a policy Polkit.";
        if (!string.IsNullOrWhiteSpace(result.DiagnosticDetails))
        {
            message = $"{message} {result.DiagnosticDetails}";
        }

        throw new InvalidOperationException(message);
    }

    private static DiagnosticRepairAction GetCatalogAction(string actionId)
    {
        return actionId switch
        {
            InstallPolkitPolicyAction => InstallPolkitPolicy,
            ValidateExternalToolsAction => ValidateExternalTools,
            UpdateGameTdbAction => UpdateGameTdb,
            _ => throw new InvalidOperationException($"Ação de reparo desconhecida: {actionId}.")
        };
    }
}
