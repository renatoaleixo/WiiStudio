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
        var currentStep = settings.FirstRunCompleted
            ? FirstRunSetupStep.Completed
            : ResolveCurrentStep(settings);

        return new FirstRunSetupStatus(
            settings.FirstRunCompleted,
            settings.FirstRunCompletedAt,
            currentStep,
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
        var libraryPath = ValidateOptionalDirectory(request.LibraryPath);
        var exportPath = ValidateOptionalDirectory(request.ExportPath);

        if (!request.Skip)
        {
            var tools = await _externalToolManager.CheckAllAsync(cancellationToken);
            summary.Add(CreateToolsSummary(tools));

            var policy = await _polkitPolicyInstaller.CheckAsync(cancellationToken);
            summary.Add($"Permissões USB: {CreatePolicySummary(policy)}");

            if (request.UserConfirmedDownloads)
            {
                var update = await _gameTdbUpdateService.UpdateAsync(new GameTdbUpdateOptions(Force: false), cancellationToken);
                summary.Add($"GameTDB: {update.Message}");
            }
            else
            {
                var updateCheck = await _gameTdbUpdateService.CheckForUpdatesAsync(cancellationToken);
                summary.Add($"GameTDB: {updateCheck.Message}");
            }
        }

        var updated = new Domain.Entities.Settings
        {
            Language = string.IsNullOrWhiteSpace(request.Language) ? settings.Language : request.Language,
            Theme = string.IsNullOrWhiteSpace(request.Theme) ? settings.Theme : request.Theme,
            AutoScanLibrary = settings.AutoScanLibrary,
            LibraryPath = string.IsNullOrWhiteSpace(libraryPath) ? settings.LibraryPath : libraryPath,
            ExportPath = string.IsNullOrWhiteSpace(exportPath) ? settings.ExportPath : exportPath,
            MaxParallelTasks = settings.MaxParallelTasks,
            EnableDiagnostics = settings.EnableDiagnostics,
            CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup,
            WitExecutablePath = settings.WitExecutablePath,
            WwtExecutablePath = settings.WwtExecutablePath,
            NKitExecutablePath = settings.NKitExecutablePath,
            DolphinExecutablePath = settings.DolphinExecutablePath,
            WitVersion = settings.WitVersion,
            WwtVersion = settings.WwtVersion,
            NKitVersion = settings.NKitVersion,
            DolphinVersion = settings.DolphinVersion,
            EmulatorShowPreviewBeforeLaunch = settings.EmulatorShowPreviewBeforeLaunch,
            EmulatorSaveLaunchHistory = settings.EmulatorSaveLaunchHistory,
            EmulatorCloseAfterLaunch = settings.EmulatorCloseAfterLaunch,
            EmulatorLastId = settings.EmulatorLastId,
            EmulatorLastGameId = settings.EmulatorLastGameId,
            EmulatorDefaultProfileId = settings.EmulatorDefaultProfileId,
            EmulatorLastUsedProfileId = settings.EmulatorLastUsedProfileId,
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

    private static FirstRunSetupStep ResolveCurrentStep(Domain.Entities.Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Language))
        {
            return FirstRunSetupStep.Language;
        }

        if (string.IsNullOrWhiteSpace(settings.Theme))
        {
            return FirstRunSetupStep.Theme;
        }

        if (string.IsNullOrWhiteSpace(settings.LibraryPath) || string.IsNullOrWhiteSpace(settings.ExportPath))
        {
            return FirstRunSetupStep.Folders;
        }

        return FirstRunSetupStep.ExternalTools;
    }

    private static string? ValidateOptionalDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"A pasta selecionada não é válida ou não pode ser acessada: {fullPath}");
        }

        return fullPath;
    }

    private static string CreateToolsSummary(IReadOnlyList<ExternalToolStatus> tools)
    {
        var required = tools
            .Where(tool => tool.ToolId is "wiimms-wit" or "wiimms-wwt")
            .ToArray();
        var configured = required.Count(tool => tool.State is ExternalToolState.Installed or ExternalToolState.Configured);
        var total = required.Length == 0 ? tools.Count : required.Length;
        return total == 0
            ? "Ferramentas: nenhuma ferramenta catalogada."
            : $"Ferramentas: {configured}/{total} essenciais configuradas.";
    }

    private static string CreatePolicySummary(PolkitPolicyInstallStatus policy)
    {
        return policy.State switch
        {
            PolkitPolicyInstallState.Installed => "policy instalada.",
            PolkitPolicyInstallState.NotLinux => "não necessária nesta plataforma.",
            PolkitPolicyInstallState.Missing => "policy pendente.",
            _ => policy.DiagnosticDetails ?? policy.MessageKey ?? policy.State.ToString()
        };
    }
}
