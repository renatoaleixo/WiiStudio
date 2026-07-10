using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class FirstRunSetupService : IFirstRunSetupService
{
    private readonly ISettingsService _settingsService;
    private readonly IExternalToolManager _externalToolManager;
    private readonly IPolkitPolicyInstaller _polkitPolicyInstaller;
    private readonly IGameTdbUpdateService _gameTdbUpdateService;
    private readonly INotificationService _notificationService;

    public FirstRunSetupService(
        ISettingsService settingsService,
        IExternalToolManager externalToolManager,
        IPolkitPolicyInstaller polkitPolicyInstaller,
        IGameTdbUpdateService gameTdbUpdateService,
        INotificationService notificationService)
    {
        _settingsService = settingsService;
        _externalToolManager = externalToolManager;
        _polkitPolicyInstaller = polkitPolicyInstaller;
        _gameTdbUpdateService = gameTdbUpdateService;
        _notificationService = notificationService;
    }

    public async Task<FirstRunSetupStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.LoadAsync(cancellationToken);
        var summary = new List<string>
        {
            $"Idioma: {settings.Language}",
            $"Tema: {settings.Theme}",
            string.IsNullOrWhiteSpace(settings.LibraryPath) ? "Biblioteca: pendente" : $"Biblioteca: {settings.LibraryPath}",
            string.IsNullOrWhiteSpace(settings.ExportPath) ? "Exportação: pendente" : $"Exportação: {settings.ExportPath}"
        };

        return new FirstRunSetupStatus(
            settings.FirstRunCompleted,
            settings.FirstRunCompletedAt,
            settings.FirstRunCompleted ? FirstRunSetupStep.Completed : FirstRunSetupStep.Welcome,
            summary);
    }

    public async Task<FirstRunSetupResult> CompleteAsync(
        FirstRunSetupRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        var settings = await _settingsService.LoadAsync(cancellationToken);
        var summary = new List<string>();

        if (!request.Skip)
        {
            await _externalToolManager.CheckAllAsync(cancellationToken);
            await _polkitPolicyInstaller.CheckAsync(cancellationToken);
            await _gameTdbUpdateService.CheckForUpdatesAsync(cancellationToken);
        }

        var updated = new Domain.Entities.Settings
        {
            Language = string.IsNullOrWhiteSpace(request.Language) ? settings.Language : request.Language,
            Theme = string.IsNullOrWhiteSpace(request.Theme) ? settings.Theme : request.Theme,
            AutoScanLibrary = settings.AutoScanLibrary,
            LibraryPath = string.IsNullOrWhiteSpace(request.LibraryPath) ? settings.LibraryPath : request.LibraryPath,
            ExportPath = string.IsNullOrWhiteSpace(request.ExportPath) ? settings.ExportPath : request.ExportPath,
            MaxParallelTasks = settings.MaxParallelTasks,
            EnableDiagnostics = settings.EnableDiagnostics,
            CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup,
            WitExecutablePath = settings.WitExecutablePath,
            WwtExecutablePath = settings.WwtExecutablePath,
            NKitExecutablePath = settings.NKitExecutablePath,
            WitVersion = settings.WitVersion,
            WwtVersion = settings.WwtVersion,
            NKitVersion = settings.NKitVersion,
            ExternalToolsLastCheckAt = settings.ExternalToolsLastCheckAt,
            CoverCachePath = settings.CoverCachePath,
            MetadataCachePath = settings.MetadataCachePath,
            FirstRunCompleted = true,
            FirstRunCompletedAt = now
        };

        await _settingsService.SaveAsync(updated, cancellationToken);
        summary.Add($"Idioma: {updated.Language}");
        summary.Add($"Tema: {updated.Theme}");
        summary.Add(string.IsNullOrWhiteSpace(updated.LibraryPath) ? "Biblioteca: não definida" : $"Biblioteca: {updated.LibraryPath}");
        summary.Add(string.IsNullOrWhiteSpace(updated.ExportPath) ? "Exportação: não definida" : $"Exportação: {updated.ExportPath}");
        summary.Add(request.Skip ? "Assistente pulado pelo usuário." : "Verificações iniciais executadas.");

        await _notificationService.PublishAsync(
            NotificationMessage.Create(NotificationType.Success, "Configuração inicial concluída", summary[^1], groupKey: "first-run"),
            cancellationToken);

        return new FirstRunSetupResult(Completed: !request.Skip, request.Skip, now, summary);
    }
}
