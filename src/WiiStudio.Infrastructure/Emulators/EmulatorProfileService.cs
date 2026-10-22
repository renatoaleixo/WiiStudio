using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Emulators;

public sealed class EmulatorProfileService : IEmulatorProfileService
{
    public const string DefaultProfileId = "dolphin-default";
    private static readonly HashSet<string> AllowedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Nenhum", "Teclado", "Controle USB/Bluetooth", "Wiimote real"
    };
    private static readonly HashSet<string> AllowedGraphicsBackends = new(StringComparer.OrdinalIgnoreCase)
    {
        "Automático", "Vulkan", "OpenGL", "Direct3D 11", "Direct3D 12", "Metal"
    };
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IEmulatorProfileRepository repository;
    private readonly IExternalToolManager externalToolManager;
    private readonly IProcessRunner processRunner;
    private readonly INotificationService? notificationService;
    private readonly ILoggingCenter? loggingCenter;

    public EmulatorProfileService(
        IEmulatorProfileRepository repository,
        IExternalToolManager externalToolManager,
        IProcessRunner processRunner,
        INotificationService? notificationService = null,
        ILoggingCenter? loggingCenter = null)
    {
        this.repository = repository;
        this.externalToolManager = externalToolManager;
        this.processRunner = processRunner;
        this.notificationService = notificationService;
        this.loggingCenter = loggingCenter;
    }

    public async Task<IReadOnlyList<EmulatorProfileDto>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDefaultProfileAsync(cancellationToken);
        return await repository.GetProfilesAsync(cancellationToken);
    }

    public async Task<EmulatorProfileDto> SaveAsync(EmulatorProfileDto profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Validate(profile);
        await EnsureDefaultProfileAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(profile.ProfileId)
            && await repository.GetByIdAsync(profile.ProfileId, cancellationToken) is { IsDefault: true }
            && !profile.IsDefault)
        {
            throw new InvalidOperationException("Defina outro perfil como padrão antes de alterar o perfil padrão atual.");
        }

        var normalized = profile with
        {
            ProfileId = string.IsNullOrWhiteSpace(profile.ProfileId) ? Guid.NewGuid().ToString("N") : profile.ProfileId.Trim(),
            EmulatorId = "dolphin",
            Name = profile.Name.Trim(),
            ExpectedController = NormalizeOptional(profile.ExpectedController),
            GraphicsBackend = NormalizeOptional(profile.GraphicsBackend),
            Notes = NormalizeOptional(profile.Notes),
            Options = FilterOptions(profile.Options),
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await repository.UpsertAsync(normalized, cancellationToken);
        await PublishAsync("Perfil de emulação salvo", $"O perfil {normalized.Name} foi salvo.", cancellationToken);
        await LogAsync("saved", normalized.ProfileId, null, cancellationToken);
        return normalized;
    }

    public async Task DeleteAsync(string profileId, CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new KeyNotFoundException("O perfil de emulação não foi encontrado.");
        if (profile.IsDefault)
        {
            throw new InvalidOperationException("Não é possível remover o perfil padrão enquanto ele estiver em uso.");
        }

        await repository.DeleteAsync(profile.ProfileId, cancellationToken);
        await LogAsync("deleted", profile.ProfileId, null, cancellationToken);
    }

    public async Task AssignToGameAsync(string gameId, string profileId, CancellationToken cancellationToken = default)
    {
        var normalizedGameId = NormalizeGameId(gameId);
        var profile = await repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new KeyNotFoundException("O perfil de emulação não foi encontrado.");
        await repository.UpsertAssignmentAsync(
            new GameEmulatorProfileAssignmentDto(normalizedGameId, profile.EmulatorId, profile.ProfileId, DateTimeOffset.UtcNow),
            cancellationToken);
        await PublishAsync("Perfil aplicado ao jogo", $"{profile.Name} foi associado a {normalizedGameId}.", cancellationToken);
        await LogAsync("assigned", profile.ProfileId, normalizedGameId, cancellationToken);
    }

    public async Task<EmulatorProfileDto> ResolveForGameAsync(
        string gameId,
        string emulatorId = "dolphin",
        CancellationToken cancellationToken = default)
    {
        await EnsureDefaultProfileAsync(cancellationToken);
        var assignment = await repository.GetAssignmentAsync(NormalizeGameId(gameId), emulatorId, cancellationToken);
        if (assignment is not null && await repository.GetByIdAsync(assignment.ProfileId, cancellationToken) is { } assigned)
        {
            return assigned;
        }

        var profiles = await repository.GetProfilesAsync(cancellationToken);
        return profiles.First(profile => profile.IsDefault && string.Equals(profile.EmulatorId, emulatorId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task ExportAsync(string profileId, string filePath, CancellationToken cancellationToken = default)
    {
        ValidateTransferPath(filePath);
        var profile = await repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new KeyNotFoundException("O perfil de emulação não foi encontrado.");
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, profile, JsonOptions, cancellationToken);
    }

    public async Task<EmulatorProfileDto> ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ValidateTransferPath(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("O arquivo de perfil não foi encontrado.", filePath);
        }

        await using var stream = File.OpenRead(filePath);
        var imported = await JsonSerializer.DeserializeAsync<EmulatorProfileDto>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("O arquivo de perfil está vazio ou inválido.");
        return await SaveAsync(imported with { ProfileId = Guid.NewGuid().ToString("N"), IsDefault = false }, cancellationToken);
    }

    public async Task OpenEmulatorConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var dolphin = await externalToolManager.CheckAsync(ExternalToolDefinitionProvider.DolphinToolId, cancellationToken);
        if (dolphin.State is not (ExternalToolState.Configured or ExternalToolState.Installed) || string.IsNullOrWhiteSpace(dolphin.ExecutablePath))
        {
            throw new InvalidOperationException("Configure o Dolphin antes de usar perfis de emulação.");
        }

        await processRunner.RunAsync(
            new ProcessRequest(dolphin.ExecutablePath, [], TimeSpan.FromSeconds(10), waitForExit: false),
            cancellationToken);
    }

    private async Task EnsureDefaultProfileAsync(CancellationToken cancellationToken)
    {
        var profiles = await repository.GetProfilesAsync(cancellationToken);
        if (profiles.Any(profile => profile.IsDefault && string.Equals(profile.EmulatorId, "dolphin", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        await repository.UpsertAsync(
            new EmulatorProfileDto(
                DefaultProfileId, "dolphin", "Padrão", true, "Nenhum", false, "Automático", null,
                new Dictionary<string, string>(), DateTimeOffset.UtcNow),
            cancellationToken);
    }

    private static void Validate(EmulatorProfileDto profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Name) || profile.Name.Trim().Length > 80)
        {
            throw new ArgumentException("O perfil de emulação está incompleto ou inválido.", nameof(profile));
        }

        if (!string.IsNullOrWhiteSpace(profile.ExpectedController) && !AllowedControllers.Contains(profile.ExpectedController.Trim()))
        {
            throw new ArgumentException("O controle esperado informado não é suportado.", nameof(profile));
        }

        if (!string.IsNullOrWhiteSpace(profile.GraphicsBackend) && !AllowedGraphicsBackends.Contains(profile.GraphicsBackend.Trim()))
        {
            throw new ArgumentException("O backend gráfico informado não é suportado.", nameof(profile));
        }
    }

    private static IReadOnlyDictionary<string, string> FilterOptions(IReadOnlyDictionary<string, string>? options)
    {
        if (options is null)
        {
            return new Dictionary<string, string>();
        }

        return options
            .Where(pair => string.Equals(pair.Key, "controllerChecklist", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeGameId(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId) || gameId.Trim().Length > 16)
        {
            throw new ArgumentException("Game ID inválido.", nameof(gameId));
        }

        return gameId.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateTransferPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || !string.Equals(Path.GetExtension(filePath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("O caminho do perfil deve apontar para um arquivo JSON válido.", nameof(filePath));
        }
    }

    private async Task PublishAsync(string title, string message, CancellationToken cancellationToken)
    {
        if (notificationService is not null)
        {
            await notificationService.PublishAsync(NotificationMessage.Create(NotificationType.Success, title, message, groupKey: "emulator-profile"), cancellationToken);
        }
    }

    private async Task LogAsync(string action, string profileId, string? gameId, CancellationToken cancellationToken)
    {
        if (loggingCenter is null)
        {
            return;
        }

        try
        {
            await loggingCenter.RecordAsync(
                new LogEntry(Guid.NewGuid(), DateTimeOffset.UtcNow, LogLevel.Information, "WiiStudio.Emulators", "EmulatorProfile", $"Emulator profile {action}.",
                    Properties: new Dictionary<string, string> { ["ProfileId"] = profileId, ["GameId"] = gameId ?? string.Empty, ["EmulatorId"] = "dolphin", ["Action"] = action }),
                cancellationToken);
        }
        catch
        {
            // Logging must not block profile management.
        }
    }
}

public sealed class ControllerDetectionService : IControllerDetectionService
{
    public Task<ControllerDetectionResult> CheckAsync(string? expectedController, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(expectedController) || string.Equals(expectedController, "Nenhum", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ControllerDetectionResult(true, "Nenhum controle externo é exigido."));
        }

        if (string.Equals(expectedController, "Teclado", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ControllerDetectionResult(true, "Teclado disponível."));
        }

        if (OperatingSystem.IsLinux())
        {
            var available = Directory.Exists("/dev/input") && Directory.EnumerateFiles("/dev/input", "js*").Any();
            return Task.FromResult(new ControllerDetectionResult(available, available ? "Controle detectado." : "O controle esperado para este perfil não foi detectado."));
        }

        return Task.FromResult(new ControllerDetectionResult(null, "Confirme se o controle esperado está conectado antes de iniciar."));
    }
}
