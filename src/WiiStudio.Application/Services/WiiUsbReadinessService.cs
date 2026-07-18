using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Services;

public sealed class WiiUsbReadinessService : IWiiUsbReadinessService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly IUsbDriveService _usbDriveService;
    private readonly IGameLibraryScanner _gameLibraryScanner;
    private readonly IExternalToolManager _externalToolManager;
    private readonly IDevicePermissionService _devicePermissionService;
    private readonly IGameMetadataRepository _metadataRepository;
    private readonly IGameAssetRepository _assetRepository;
    private readonly ITaskManager _taskManager;

    public WiiUsbReadinessService(
        IUsbDriveService usbDriveService,
        IGameLibraryScanner gameLibraryScanner,
        IExternalToolManager externalToolManager,
        IDevicePermissionService devicePermissionService,
        IGameMetadataRepository metadataRepository,
        IGameAssetRepository assetRepository,
        ITaskManager taskManager)
    {
        _usbDriveService = usbDriveService;
        _gameLibraryScanner = gameLibraryScanner;
        _externalToolManager = externalToolManager;
        _devicePermissionService = devicePermissionService;
        _metadataRepository = metadataRepository;
        _assetRepository = assetRepository;
        _taskManager = taskManager;
    }

    public async Task<WiiUsbReadinessResult> CheckAsync(
        Guid driveId,
        CancellationToken cancellationToken = default)
    {
        var taskId = await _taskManager.EnqueueAsync(
            new TaskRequest(ManagedTaskType.Verification, "Verificação Wii USB", "Validando dispositivo de ponta a ponta."),
            cancellationToken);
        var items = new List<WiiUsbReadinessCheckItem>();

        try
        {
            await _taskManager.UpdateProgressAsync(taskId, 0.05, ManagedTaskStatus.Running, cancellationToken);
            var drives = await _usbDriveService.DetectAsync(cancellationToken);
            var drive = drives.FirstOrDefault(candidate => candidate.Id == driveId);
            if (drive is null)
            {
                items.Add(new("device", "Dispositivo detectado", ReadinessState.NotReady, "Dispositivo não encontrado.", "Conecte ou atualize a lista de dispositivos.", null));
                var missingResult = CreateResult(driveId.ToString(), items);
                await _taskManager.FailAsync(taskId, "Dispositivo não encontrado.", CancellationToken.None);
                return missingResult;
            }

            items.Add(CheckDevice(drive));
            await _taskManager.UpdateProgressAsync(taskId, 0.2, ManagedTaskStatus.Running, cancellationToken);
            items.Add(await RunCheckAsync(
                () => CheckWiimmsAsync(cancellationToken),
                new("wiimms", "Wiimms funcionando", ReadinessState.NotReady, "Não foi possível validar Wiimms.", "Valide ou configure as ferramentas Wiimms.", DiagnosticsRepairService.ValidateExternalToolsAction)));
            await _taskManager.UpdateProgressAsync(taskId, 0.35, ManagedTaskStatus.Running, cancellationToken);
            items.Add(await RunCheckAsync(
                () => CheckPermissionAsync(drive, cancellationToken),
                new("permission", "Permissão USB", ReadinessState.Warning, "Não foi possível validar permissão USB.", "Abra o diagnóstico e corrija permissões/policy.", DiagnosticsRepairService.InstallPolkitPolicyAction)));
            items.Add(CheckFileSystem(drive));
            items.Add(CheckFreeSpace(drive));

            IReadOnlyList<GameDto> games = [];
            try
            {
                games = (await _gameLibraryScanner.ScanAsync(drive, cancellationToken)).Games;
            }
            catch
            {
                items.Add(new("games", "Jogos detectados", ReadinessState.Warning, "Não foi possível listar jogos.", "Valide Wiimms e permissões.", DiagnosticsRepairService.ValidateExternalToolsAction));
            }

            if (games.Count > 0)
            {
                items.Add(new("games", "Jogos detectados", ReadinessState.Ready, $"{games.Count} jogo(s) encontrado(s).", null, null));
                items.Add(CheckGameIds(games));
                await _taskManager.UpdateProgressAsync(taskId, 0.7, ManagedTaskStatus.Running, cancellationToken);
                items.Add(await CheckMetadataAndAssetsAsync(games, cancellationToken));
            }
            else if (!items.Any(item => item.CheckId == "games"))
            {
                items.Add(new("games", "Jogos detectados", ReadinessState.Warning, "Nenhum jogo encontrado.", "Carregue jogos no dispositivo ou atualize a biblioteca.", null));
            }

            await _taskManager.UpdateProgressAsync(taskId, 0.95, ManagedTaskStatus.Running, cancellationToken);
            var result = CreateResult(drive.Id.ToString(), items);
            if (result.State == ReadinessState.NotReady)
            {
                await _taskManager.FailAsync(taskId, "Dispositivo não está pronto para Wii.", CancellationToken.None);
            }
            else
            {
                await _taskManager.CompleteAsync(taskId, CancellationToken.None);
            }

            return result;
        }
        catch
        {
            await _taskManager.FailAsync(taskId, "Falha na verificação Wii USB.", CancellationToken.None);
            throw;
        }
    }

    public async Task ExportAsync(
        WiiUsbReadinessResult result,
        string outputFile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);
        var directory = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (string.Equals(Path.GetExtension(outputFile), ".txt", StringComparison.OrdinalIgnoreCase))
        {
            await File.WriteAllLinesAsync(outputFile, CreateTextReport(result), cancellationToken);
            return;
        }

        await using var stream = File.Create(outputFile);
        await JsonSerializer.SerializeAsync(stream, result, JsonOptions, cancellationToken);
    }

    private static async Task<WiiUsbReadinessCheckItem> RunCheckAsync(
        Func<Task<WiiUsbReadinessCheckItem>> check,
        WiiUsbReadinessCheckItem fallback)
    {
        try
        {
            return await check();
        }
        catch
        {
            return fallback;
        }
    }

    private static WiiUsbReadinessCheckItem CheckDevice(UsbDrive drive)
    {
        return drive.IsMounted
            ? new("device", "Dispositivo detectado", ReadinessState.Ready, $"{drive.Label} em {drive.MountPoint}.", null, null)
            : new("device", "Dispositivo detectado", ReadinessState.NotReady, $"{drive.Label} não está montado.", "Monte o dispositivo USB.", null);
    }

    private async Task<WiiUsbReadinessCheckItem> CheckWiimmsAsync(CancellationToken cancellationToken)
    {
        var wit = await _externalToolManager.CheckAsync("wiimms-wit", cancellationToken);
        var wwt = await _externalToolManager.CheckAsync("wiimms-wwt", cancellationToken);
        var ok = IsToolReady(wit) && IsToolReady(wwt);
        return ok
            ? new("wiimms", "Wiimms funcionando", ReadinessState.Ready, $"wit {wit.Version ?? "-"}; wwt {wwt.Version ?? "-"}.", null, null)
            : new("wiimms", "Wiimms funcionando", ReadinessState.NotReady, "wit/wwt não estão configurados.", "Valide ou configure as ferramentas Wiimms.", DiagnosticsRepairService.ValidateExternalToolsAction);
    }

    private async Task<WiiUsbReadinessCheckItem> CheckPermissionAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        var tool = await _externalToolManager.CheckAsync("wiimms-wwt", cancellationToken);
        if (string.IsNullOrWhiteSpace(tool.ExecutablePath))
        {
            return new("permission", "Permissão USB", ReadinessState.Warning, "wwt não configurado para testar permissão.", "Configure Wiimms.", DiagnosticsRepairService.ValidateExternalToolsAction);
        }

        var status = await _devicePermissionService.CheckAsync(
            new DevicePermissionRequest("wwt", tool.ExecutablePath, drive.DevicePath, PrivilegedDeviceOperation.ReadWbfsMetadata),
            cancellationToken);
        return status.State == PrivilegedAccessState.Authorized
            ? new("permission", "Permissão USB", ReadinessState.Ready, status.DiagnosticDetails ?? "Permissão autorizada.", null, null)
            : new("permission", "Permissão USB", ReadinessState.Warning, status.DiagnosticDetails ?? status.State.ToString(), "Corrija permissões/policy.", DiagnosticsRepairService.InstallPolkitPolicyAction);
    }

    private static WiiUsbReadinessCheckItem CheckFileSystem(UsbDrive drive)
    {
        if (drive.FileSystem is not (UsbDriveFileSystem.Wbfs or UsbDriveFileSystem.Fat32 or UsbDriveFileSystem.Ntfs))
        {
            return new("filesystem", "Filesystem suportado", ReadinessState.NotReady, drive.FileSystem.ToString(), "Use WBFS, FAT32 ou NTFS.", null);
        }

        if (drive.FileSystem is UsbDriveFileSystem.Fat32 or UsbDriveFileSystem.Ntfs
            && Directory.Exists(drive.MountPoint)
            && !Directory.Exists(Path.Combine(drive.MountPoint, "wbfs")))
        {
            return new("filesystem", "Filesystem suportado", ReadinessState.Warning, $"{drive.FileSystem}; pasta /wbfs ausente.", "Crie a pasta /wbfs antes de usar no Wii.", null);
        }

        return new("filesystem", "Filesystem suportado", ReadinessState.Ready, drive.FileSystem.ToString(), null, null);
    }

    private static WiiUsbReadinessCheckItem CheckFreeSpace(UsbDrive drive)
    {
        return drive.FreeSpace > 512L * 1024L * 1024L
            ? new("space", "Espaço livre", ReadinessState.Ready, $"{drive.FreeSpace} bytes livres.", null, null)
            : new("space", "Espaço livre", ReadinessState.Warning, $"{drive.FreeSpace} bytes livres.", "Libere espaço antes de novas importações.", null);
    }

    private static WiiUsbReadinessCheckItem CheckGameIds(IReadOnlyList<GameDto> games)
    {
        var invalidIds = games.Count(game => !IsValidGameId(game.Id));
        var duplicateIds = games
            .GroupBy(game => game.Id, StringComparer.OrdinalIgnoreCase)
            .Count(group => group.Count() > 1);

        if (invalidIds == 0 && duplicateIds == 0)
        {
            return new("game-ids", "Game IDs válidos", ReadinessState.Ready, $"{games.Count} ID(s) validado(s).", null, null);
        }

        return new(
            "game-ids",
            "Game IDs válidos",
            ReadinessState.Warning,
            $"{invalidIds} ID(s) inválido(s); {duplicateIds} duplicidade(s).",
            "Verifique a biblioteca e reimporte jogos com IDs inconsistentes.",
            null);
    }

    private async Task<WiiUsbReadinessCheckItem> CheckMetadataAndAssetsAsync(IReadOnlyList<GameDto> games, CancellationToken cancellationToken)
    {
        var missingMetadata = 0;
        var missingFrontCovers = 0;
        foreach (var game in games)
        {
            if (await _metadataRepository.GetAsync(game.Id, cancellationToken) is null)
            {
                missingMetadata++;
            }

            var assets = await _assetRepository.GetByGameIdAsync(game.Id, cancellationToken);
            if (!assets.Any(asset => asset.Key.Kind == CoverAssetKind.FrontCover && File.Exists(asset.LocalPath)))
            {
                missingFrontCovers++;
            }
        }

        return missingMetadata == 0 && missingFrontCovers == 0
            ? new("assets-metadata", "Capas e metadados", ReadinessState.Ready, "Capas e metadados disponíveis.", null, null)
            : new("assets-metadata", "Capas e metadados", ReadinessState.Warning, $"{missingMetadata} sem metadados; {missingFrontCovers} sem capa frontal.", "Baixe capas e sincronize metadados.", DiagnosticsRepairService.UpdateGameTdbAction);
    }

    private static WiiUsbReadinessResult CreateResult(string deviceId, IReadOnlyList<WiiUsbReadinessCheckItem> items)
    {
        var state = items.Any(item => item.State == ReadinessState.NotReady)
            ? ReadinessState.NotReady
            : items.Any(item => item.State == ReadinessState.Warning)
                ? ReadinessState.Warning
                : ReadinessState.Ready;
        return new WiiUsbReadinessResult(deviceId, state, items, DateTimeOffset.UtcNow);
    }

    private static IReadOnlyList<string> CreateTextReport(WiiUsbReadinessResult result)
    {
        var lines = new List<string>
        {
            "WiiStudio - Relatorio de Prontidao Wii USB",
            $"Dispositivo: {result.DeviceId}",
            $"Resultado: {result.State}",
            $"Gerado em: {result.CheckedAt:yyyy-MM-dd HH:mm:ss zzz}",
            $"Plataforma: {Environment.OSVersion}",
            string.Empty
        };

        foreach (var item in result.Items)
        {
            lines.Add($"[{item.State}] {item.Title} ({item.CheckId})");
            if (!string.IsNullOrWhiteSpace(item.Details))
            {
                lines.Add($"Detalhes: {item.Details}");
            }

            if (!string.IsNullOrWhiteSpace(item.RecommendedAction))
            {
                lines.Add($"Acao recomendada: {item.RecommendedAction}");
            }

            if (!string.IsNullOrWhiteSpace(item.RepairActionId))
            {
                lines.Add($"Reparo: {item.RepairActionId}");
            }

            lines.Add(string.Empty);
        }

        return lines;
    }

    private static bool IsToolReady(ExternalToolStatus status)
    {
        return status.State is ExternalToolState.Installed or ExternalToolState.Configured;
    }

    private static bool IsValidGameId(string gameId)
    {
        return gameId.Length == 6 && gameId.All(char.IsLetterOrDigit);
    }
}
