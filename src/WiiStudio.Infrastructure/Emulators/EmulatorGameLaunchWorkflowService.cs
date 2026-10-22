using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Security;

namespace WiiStudio.Infrastructure.Emulators;

public sealed class EmulatorGameLaunchWorkflowService : IEmulatorGameLaunchWorkflowService
{
    private static readonly HashSet<FileFormat> SupportedFormats =
    [
        FileFormat.Iso,
        FileFormat.Wbfs,
        FileFormat.Rvz
    ];

    private readonly IGameLibraryService gameLibraryService;
    private readonly IExternalToolManager externalToolManager;
    private readonly IProcessRunner processRunner;
    private readonly ISafePathService safePathService;
    private readonly ITaskManager? taskManager;
    private readonly INotificationService? notificationService;
    private readonly IEmulatorLaunchHistoryRepository? launchHistoryRepository;
    private readonly ILoggingCenter? loggingCenter;
    private readonly IEmulatorProfileService? emulatorProfileService;
    private readonly IControllerDetectionService? controllerDetectionService;

    public EmulatorGameLaunchWorkflowService(
        IGameLibraryService gameLibraryService,
        IExternalToolManager externalToolManager,
        IProcessRunner processRunner,
        ISafePathService safePathService,
        ITaskManager? taskManager = null,
        INotificationService? notificationService = null,
        IEmulatorLaunchHistoryRepository? launchHistoryRepository = null,
        ILoggingCenter? loggingCenter = null,
        IEmulatorProfileService? emulatorProfileService = null,
        IControllerDetectionService? controllerDetectionService = null)
    {
        this.gameLibraryService = gameLibraryService;
        this.externalToolManager = externalToolManager;
        this.processRunner = processRunner;
        this.safePathService = safePathService;
        this.taskManager = taskManager;
        this.notificationService = notificationService;
        this.launchHistoryRepository = launchHistoryRepository;
        this.loggingCenter = loggingCenter;
        this.emulatorProfileService = emulatorProfileService;
        this.controllerDetectionService = controllerDetectionService;
    }

    public async Task<EmulatorLaunchPreview> CreatePreviewAsync(
        EmulatorLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsDolphin(request.EmulatorId))
        {
            return Blocked(request.GameId, request.EmulatorId, "Emulador não suportado.");
        }

        var game = await FindGameAsync(request.GameId, cancellationToken);
        if (game is null)
        {
            return Blocked(request.GameId, request.EmulatorId, "Jogo não encontrado na biblioteca.");
        }

        if (string.IsNullOrWhiteSpace(game.FilePath))
        {
            return Blocked(game, request.EmulatorId, "Este jogo não possui arquivo local disponível para execução.");
        }

        if (!SupportedFormats.Contains(game.Format))
        {
            return Blocked(game, request.EmulatorId, "O formato deste jogo ainda não está habilitado para execução no emulador.");
        }

        var gamePath = safePathService.ValidateReadableFile(game.FilePath);
        if (!gamePath.Success || gamePath.NormalizedPath is null)
        {
            return Blocked(game, request.EmulatorId, "O arquivo do jogo não foi encontrado no caminho original.");
        }

        var emulator = await externalToolManager.CheckAsync(ExternalToolDefinitionProvider.DolphinToolId, cancellationToken);
        if (emulator.State is not (ExternalToolState.Installed or ExternalToolState.Configured)
            || string.IsNullOrWhiteSpace(emulator.ExecutablePath))
        {
            return new EmulatorLaunchPreview(
                false,
                "Configure o Dolphin antes de jogar no PC.",
                game.Id,
                game.Title,
                gamePath.NormalizedPath,
                request.EmulatorId,
                emulator.Version,
                null,
                null,
                null,
                []);
        }

        EmulatorProfileDto? profile = null;
        var warnings = new List<string>();
        if (emulatorProfileService is not null)
        {
            profile = await emulatorProfileService.ResolveForGameAsync(game.Id, request.EmulatorId, cancellationToken);
            if (controllerDetectionService is not null)
            {
                var controller = await controllerDetectionService.CheckAsync(profile.ExpectedController, cancellationToken);
                if (controller.IsAvailable is not true)
                {
                    warnings.Add(controller.Message);
                }
            }
        }

        return new EmulatorLaunchPreview(
            true,
            null,
            game.Id,
            game.Title,
            gamePath.NormalizedPath,
            request.EmulatorId,
            emulator.Version,
            profile?.ProfileId,
            profile?.Name ?? "Padrão",
            profile?.ExpectedController,
            warnings);
    }

    public async Task<EmulatorLaunchResult> ExecuteAsync(
        EmulatorLaunchExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var preview = await CreatePreviewAsync(new EmulatorLaunchRequest(request.GameId, request.EmulatorId), cancellationToken);
        if (!request.UserConfirmed)
        {
            const string cancellationMessage = "Execução cancelada pelo usuário.";
            await SaveHistoryAsync(request.GameId, request.EmulatorId, preview.GameFilePath, "Cancelled", cancellationMessage, request.SaveHistory, cancellationToken);
            return new EmulatorLaunchResult(false, request.GameId, request.EmulatorId, DateTimeOffset.UtcNow, cancellationMessage);
        }

        if (!preview.CanLaunch || string.IsNullOrWhiteSpace(preview.GameFilePath))
        {
            return await FailAsync(request.GameId, request.EmulatorId, preview.GameFilePath, "Blocked", preview.BlockReason ?? "Não foi possível iniciar o jogo no emulador.", request.SaveHistory, cancellationToken);
        }

        var emulator = await externalToolManager.CheckAsync(ExternalToolDefinitionProvider.DolphinToolId, cancellationToken);
        if (string.IsNullOrWhiteSpace(emulator.ExecutablePath))
        {
            return await FailAsync(preview.GameId, request.EmulatorId, preview.GameFilePath, "Blocked", "Configure o Dolphin antes de jogar no PC.", request.SaveHistory, cancellationToken);
        }

        Guid? taskId = await EnqueueLaunchTaskAsync(preview, cancellationToken);
        try
        {
            foreach (var warning in preview.Warnings)
            {
                await PublishAsync(NotificationType.Warning, "Verifique o controle", warning, cancellationToken);
            }

            await processRunner.RunAsync(
                new ProcessRequest(
                    emulator.ExecutablePath,
                    [$"--exec={preview.GameFilePath}"],
                    TimeSpan.FromSeconds(10),
                    waitForExit: false),
                cancellationToken);

            await CompleteTaskAsync(taskId, cancellationToken);
            await SaveHistoryAsync(preview.GameId, request.EmulatorId, preview.GameFilePath, "Started", null, request.SaveHistory, cancellationToken);
            await PublishAsync(NotificationType.Success, "Jogo iniciado no Dolphin", $"{preview.GameTitle ?? preview.GameId} foi enviado para o emulador.", cancellationToken);
            return new EmulatorLaunchResult(true, preview.GameId, request.EmulatorId, DateTimeOffset.UtcNow);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await FailTaskAsync(taskId, exception.Message);
            return await FailAsync(preview.GameId, request.EmulatorId, preview.GameFilePath, "Failed", "Não foi possível iniciar o jogo no emulador.", request.SaveHistory, cancellationToken);
        }
    }

    private async Task<GameDto?> FindGameAsync(string gameId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            return null;
        }

        var normalized = gameId.Trim().ToUpperInvariant();
        var games = await gameLibraryService.GetGamesAsync(null, cancellationToken);
        return games.FirstOrDefault(game => string.Equals(game.Id, normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static EmulatorLaunchPreview Blocked(string gameId, string emulatorId, string reason) =>
        new(false, reason, gameId, null, null, emulatorId, null, null, null, null, []);

    private static EmulatorLaunchPreview Blocked(GameDto game, string emulatorId, string reason) =>
        new(false, reason, game.Id, game.Title, game.FilePath, emulatorId, null, null, null, null, []);

    private static bool IsDolphin(string emulatorId) =>
        string.Equals(emulatorId, ExternalToolDefinitionProvider.DolphinToolId, StringComparison.OrdinalIgnoreCase);

    private async Task<Guid?> EnqueueLaunchTaskAsync(EmulatorLaunchPreview preview, CancellationToken cancellationToken)
    {
        if (taskManager is null)
        {
            return null;
        }

        return await taskManager.EnqueueAsync(
            new TaskRequest(
                ManagedTaskType.Other,
                "Iniciando jogo no Dolphin",
                $"{preview.GameTitle ?? preview.GameId}"),
            cancellationToken);
    }

    private async Task CompleteTaskAsync(Guid? taskId, CancellationToken cancellationToken)
    {
        if (taskManager is not null && taskId is not null)
        {
            await taskManager.CompleteAsync(taskId.Value, cancellationToken);
        }
    }

    private async Task FailTaskAsync(Guid? taskId, string message)
    {
        if (taskManager is not null && taskId is not null)
        {
            await taskManager.FailAsync(taskId.Value, message, CancellationToken.None);
        }
    }

    private async Task<EmulatorLaunchResult> FailAsync(
        string gameId,
        string emulatorId,
        string? filePath,
        string result,
        string message,
        bool saveHistory,
        CancellationToken cancellationToken)
    {
        await SaveHistoryAsync(gameId, emulatorId, filePath, result, message, saveHistory, cancellationToken);
        await PublishAsync(NotificationType.Warning, "Não foi possível iniciar o jogo", message, cancellationToken);
        return new EmulatorLaunchResult(false, gameId, emulatorId, DateTimeOffset.UtcNow, message);
    }

    private async Task SaveHistoryAsync(
        string gameId,
        string emulatorId,
        string? filePath,
        string result,
        string? errorMessage,
        bool saveHistory,
        CancellationToken cancellationToken)
    {
        await RecordLogAsync(gameId, emulatorId, result, errorMessage, cancellationToken);
        if (!saveHistory || launchHistoryRepository is null)
        {
            return;
        }

        await launchHistoryRepository.AppendAsync(
            new EmulatorLaunchHistoryEntry(
                Guid.NewGuid(),
                gameId.Trim().ToUpperInvariant(),
                emulatorId,
                filePath,
                DateTimeOffset.UtcNow,
                null,
                result,
                errorMessage),
            cancellationToken);
    }

    private async Task RecordLogAsync(
        string gameId,
        string emulatorId,
        string result,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        if (loggingCenter is null)
        {
            return;
        }

        try
        {
            await loggingCenter.RecordAsync(
                new LogEntry(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    string.Equals(result, "Started", StringComparison.Ordinal) ? LogLevel.Information : LogLevel.Warning,
                    "WiiStudio.Emulators",
                    "EmulatorLaunch",
                    $"Emulator launch result: {result}.",
                    OperationId: Guid.NewGuid().ToString("N"),
                    Properties: new Dictionary<string, string>
                    {
                        ["GameId"] = gameId,
                        ["EmulatorId"] = emulatorId,
                        ["Command"] = "dolphin --exec=<library-file>",
                        ["Result"] = result,
                        ["FailureReason"] = errorMessage ?? string.Empty
                    }),
                cancellationToken);
        }
        catch
        {
            // Logging must not prevent a game from launching.
        }
    }

    private async Task PublishAsync(
        NotificationType type,
        string title,
        string message,
        CancellationToken cancellationToken)
    {
        if (notificationService is not null)
        {
            await notificationService.PublishAsync(
                NotificationMessage.Create(type, title, message, groupKey: "emulator-launch"),
                cancellationToken);
        }
    }
}
