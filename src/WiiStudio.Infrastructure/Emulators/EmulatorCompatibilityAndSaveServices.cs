using System.IO.Compression;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Emulators;

public sealed class EmulatorSaveOptions
{
    public string BackupDirectory { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WiiStudio", "SaveBackups");

    public IReadOnlyList<string>? UserDirectoryRoots { get; init; }
}

public sealed class EmulatorCompatibilityService : IEmulatorCompatibilityService
{
    private readonly IEmulatorCompatibilityRepository repository;
    private readonly INotificationService? notifications;

    public EmulatorCompatibilityService(IEmulatorCompatibilityRepository repository, INotificationService? notifications = null)
    {
        this.repository = repository;
        this.notifications = notifications;
    }

    public async Task<EmulatorCompatibilityDto> GetAsync(string gameId, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeGameId(gameId);
        return await repository.GetAsync(normalized, "dolphin", cancellationToken)
            ?? new EmulatorCompatibilityDto(normalized, "dolphin", LocalCompatibilityStatus.NotTested, null, null, DateTimeOffset.UtcNow);
    }

    public async Task<EmulatorCompatibilityDto> SaveAsync(EmulatorCompatibilityDto compatibility, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(compatibility);
        if (compatibility.Notes?.Length > 4000)
        {
            throw new ArgumentException("As notas de compatibilidade devem ter no máximo 4000 caracteres.", nameof(compatibility));
        }

        var normalized = compatibility with
        {
            GameId = NormalizeGameId(compatibility.GameId),
            EmulatorId = "dolphin",
            Notes = string.IsNullOrWhiteSpace(compatibility.Notes) ? null : compatibility.Notes.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await repository.UpsertAsync(normalized, cancellationToken);
        if (notifications is not null)
        {
            await notifications.PublishAsync(NotificationMessage.Create(NotificationType.Success, "Compatibilidade local atualizada", $"As informações de {normalized.GameId} foram salvas.", groupKey: "emulator-compatibility"), cancellationToken);
        }

        return normalized;
    }

    private static string NormalizeGameId(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId) || gameId.Trim().Length > 16)
        {
            throw new ArgumentException("Game ID inválido.", nameof(gameId));
        }

        return gameId.Trim().ToUpperInvariant();
    }
}

public sealed class EmulatorSaveManagementService : IEmulatorSaveManagementService
{
    private readonly EmulatorSaveOptions options;
    private readonly IExternalToolManager externalTools;
    private readonly IProcessRunner processRunner;
    private readonly ISaveBackupRepository backupRepository;
    private readonly ITaskManager? tasks;
    private readonly INotificationService? notifications;
    private readonly ISettingsService? settingsService;
    private readonly ILoggingCenter? loggingCenter;

    public EmulatorSaveManagementService(
        EmulatorSaveOptions options,
        IExternalToolManager externalTools,
        IProcessRunner processRunner,
        ISaveBackupRepository backupRepository,
        ITaskManager? tasks = null,
        INotificationService? notifications = null,
        ISettingsService? settingsService = null,
        ILoggingCenter? loggingCenter = null)
    {
        this.options = options;
        this.externalTools = externalTools;
        this.processRunner = processRunner;
        this.backupRepository = backupRepository;
        this.tasks = tasks;
        this.notifications = notifications;
        this.settingsService = settingsService;
        this.loggingCenter = loggingCenter;
    }

    public async Task<SaveLocationResult> LocateSavesAsync(string gameId, string emulatorId = "dolphin", CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeGameId(gameId);
        await EnsureDolphinAsync(cancellationToken);
        foreach (var root in GetDolphinUserRoots())
        {
            var fullRoot = Path.GetFullPath(root);
            foreach (var candidate in GetSaveCandidates(fullRoot, normalized))
            {
                var fullCandidate = Path.GetFullPath(candidate);
                if (IsWithin(fullCandidate, fullRoot) && Directory.Exists(fullCandidate))
                {
                    return new SaveLocationResult(true, normalized, emulatorId, fullCandidate, "Pasta de saves localizada.");
                }
            }
        }

        return new SaveLocationResult(false, normalized, emulatorId, null, "Não foi possível localizar saves para este jogo.");
    }

    public async Task OpenSavesFolderAsync(string gameId, bool userConfirmed, CancellationToken cancellationToken = default)
    {
        var settings = settingsService is null ? null : await settingsService.GetAsync(cancellationToken);
        if (!userConfirmed && settings?.EmulatorOpenSaveFolderRequiresConfirmation is not false)
        {
            throw new InvalidOperationException("Confirme a abertura da pasta de saves.");
        }

        var location = await LocateSavesAsync(gameId, cancellationToken: cancellationToken);
        if (!location.Found || location.SavePath is null)
        {
            throw new DirectoryNotFoundException(location.Message);
        }

        var (executable, arguments) = OperatingSystem.IsWindows()
            ? ("explorer.exe", new[] { location.SavePath })
            : OperatingSystem.IsMacOS()
                ? ("open", new[] { location.SavePath })
                : ("xdg-open", new[] { location.SavePath });
        await processRunner.RunAsync(new ProcessRequest(executable, arguments, TimeSpan.FromSeconds(10), waitForExit: false), cancellationToken);
    }

    public async Task<SaveBackupResult> BackupSavesAsync(SaveBackupRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var settings = settingsService is null ? null : await settingsService.GetAsync(cancellationToken);
        if (!request.UserConfirmed && settings?.EmulatorSaveBackupRequiresConfirmation is not false)
        {
            return new SaveBackupResult(false, null, 0, DateTimeOffset.UtcNow, "Confirme o backup dos saves antes de continuar.");
        }

        var location = await LocateSavesAsync(request.GameId, request.EmulatorId, cancellationToken);
        if (!location.Found || location.SavePath is null)
        {
            return new SaveBackupResult(false, null, 0, DateTimeOffset.UtcNow, location.Message);
        }

        var destination = ResolveDestination(request.DestinationFile, request.GameId, settings?.EmulatorSaveBackupDirectory);
        if (IsWithin(destination, location.SavePath))
        {
            throw new InvalidOperationException("O caminho de backup não pode ficar dentro da pasta original de saves.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        Guid? taskId = tasks is null ? null : await tasks.EnqueueAsync(new TaskRequest(ManagedTaskType.Other, "Backup de saves", request.GameId), cancellationToken);
        var tempFile = Path.Combine(Path.GetDirectoryName(destination)!, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.tmp");
        try
        {
            ZipFile.CreateFromDirectory(location.SavePath, tempFile, CompressionLevel.Optimal, includeBaseDirectory: true);
            File.Move(tempFile, destination, overwrite: false);
            var size = new FileInfo(destination).Length;
            var createdAt = DateTimeOffset.UtcNow;
            await backupRepository.AppendAsync(new SaveBackupEntry(Guid.NewGuid(), request.GameId.Trim().ToUpperInvariant(), request.EmulatorId, location.SavePath, destination, size, true, null, createdAt), cancellationToken);
            await LogBackupAsync(request.GameId, request.EmulatorId, Path.GetFileName(destination), "Success", cancellationToken);
            if (tasks is not null && taskId is not null) await tasks.CompleteAsync(taskId.Value, cancellationToken);
            if (notifications is not null) await notifications.PublishAsync(NotificationMessage.Create(NotificationType.Success, "Backup de saves concluído", Path.GetFileName(destination), groupKey: "emulator-save-backup"), cancellationToken);
            return new SaveBackupResult(true, destination, size, createdAt, "Backup de saves concluído.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (tasks is not null && taskId is not null) await tasks.FailAsync(taskId.Value, exception.Message, CancellationToken.None);
            await backupRepository.AppendAsync(new SaveBackupEntry(Guid.NewGuid(), request.GameId.Trim().ToUpperInvariant(), request.EmulatorId, location.SavePath, destination, 0, false, exception.Message, DateTimeOffset.UtcNow), CancellationToken.None);
            await LogBackupAsync(request.GameId, request.EmulatorId, Path.GetFileName(destination), "Failed", CancellationToken.None);
            return new SaveBackupResult(false, null, 0, DateTimeOffset.UtcNow, "Não foi possível criar backup dos saves.");
        }
    }

    private async Task EnsureDolphinAsync(CancellationToken cancellationToken)
    {
        var status = await externalTools.CheckAsync(ExternalToolDefinitionProvider.DolphinToolId, cancellationToken);
        if (status.State is not (ExternalToolState.Configured or ExternalToolState.Installed))
        {
            throw new InvalidOperationException("Configure o Dolphin antes de gerenciar saves de emulação.");
        }
    }

    private string ResolveDestination(string destination, string gameId, string? configuredDirectory)
    {
        var path = string.IsNullOrWhiteSpace(destination)
            ? Path.Combine(configuredDirectory ?? options.BackupDirectory, $"{gameId.Trim().ToUpperInvariant()}-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.zip")
            : destination;
        var fullPath = Path.GetFullPath(path);
        if (!string.Equals(Path.GetExtension(fullPath), ".zip", StringComparison.OrdinalIgnoreCase) || File.Exists(fullPath))
        {
            throw new InvalidOperationException("Escolha um novo arquivo ZIP para o backup.");
        }

        return fullPath;
    }

    private IEnumerable<string> GetDolphinUserRoots()
    {
        if (options.UserDirectoryRoots is { Count: > 0 })
        {
            foreach (var configuredRoot in options.UserDirectoryRoots) yield return configuredRoot;
            yield break;
        }

        var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (OperatingSystem.IsWindows())
        {
            yield return Path.Combine(documents, "Dolphin Emulator");
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dolphin Emulator");
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return Path.Combine(user, "Library", "Application Support", "Dolphin");
        }
        else
        {
            yield return Path.Combine(user, ".local", "share", "dolphin-emu");
            yield return Path.Combine(user, ".var", "app", "org.DolphinEmu.dolphin-emu", "data", "dolphin-emu");
        }
    }

    private static IEnumerable<string> GetSaveCandidates(string root, string gameId)
    {
        var titleCode = gameId.Length >= 4 ? gameId[..4] : gameId;
        var titleHex = Convert.ToHexString(System.Text.Encoding.ASCII.GetBytes(titleCode)).ToLowerInvariant();
        yield return Path.Combine(root, "Wii", "title", "00010000", titleHex, "data");
        yield return Path.Combine(root, "Wii", "title", "00010004", titleHex, "data");
        yield return Path.Combine(root, "Saves", gameId);
    }

    private static bool IsWithin(string path, string root)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(path).StartsWith(normalizedRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private static string NormalizeGameId(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId) || gameId.Trim().Length is < 4 or > 16)
        {
            throw new ArgumentException("Game ID inválido.", nameof(gameId));
        }

        return gameId.Trim().ToUpperInvariant();
    }

    private async Task LogBackupAsync(string gameId, string emulatorId, string backupFileName, string result, CancellationToken cancellationToken)
    {
        if (loggingCenter is null) return;
        try
        {
            await loggingCenter.RecordAsync(
                new LogEntry(Guid.NewGuid(), DateTimeOffset.UtcNow, result == "Success" ? LogLevel.Information : LogLevel.Warning,
                    "WiiStudio.Emulators", "SaveBackup", $"Save backup result: {result}.", OperationId: Guid.NewGuid().ToString("N"),
                    Properties: new Dictionary<string, string>
                    {
                        ["GameId"] = gameId,
                        ["EmulatorId"] = emulatorId,
                        ["BackupFile"] = backupFileName,
                        ["Result"] = result
                    }),
                cancellationToken);
        }
        catch
        {
            // Logging must not block save backups.
        }
    }
}
