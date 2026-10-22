using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Emulators;
using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Security;

namespace WiiStudio.Infrastructure.Tests.Emulators;

public sealed class EmulatorGameLaunchWorkflowServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wiistudio-emulator-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task CreatePreviewAsync_ShouldRequireConfiguredDolphin()
    {
        var gamePath = CreateFile("games/RMCP01.wbfs");
        var service = CreateService(
            [CreateGame("RMCP01", gamePath)],
            new FakeExternalToolManager(ExternalToolState.NotInstalled));

        var preview = await service.CreatePreviewAsync(new EmulatorLaunchRequest("RMCP01"));

        preview.CanLaunch.Should().BeFalse();
        preview.BlockReason.Should().Be("Configure o Dolphin antes de jogar no PC.");
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockUnsupportedFormat()
    {
        var gamePath = CreateFile("games/RMCP01.nkit.iso");
        var service = CreateService(
            [CreateGame("RMCP01", gamePath) with { Format = FileFormat.NKit }],
            new FakeExternalToolManager(ExternalToolState.Configured, "/usr/bin/dolphin-emu", "2409"));

        var preview = await service.CreatePreviewAsync(new EmulatorLaunchRequest("RMCP01"));

        preview.CanLaunch.Should().BeFalse();
        preview.BlockReason.Should().Be("O formato deste jogo ainda não está habilitado para execução no emulador.");
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldBlockMissingGameFile()
    {
        var service = CreateService(
            [CreateGame("RMCP01", Path.Combine(root, "missing.wbfs"))],
            new FakeExternalToolManager(ExternalToolState.Configured, "/usr/bin/dolphin-emu", "2409"));

        var preview = await service.CreatePreviewAsync(new EmulatorLaunchRequest("RMCP01"));

        preview.CanLaunch.Should().BeFalse();
        preview.BlockReason.Should().Be("O arquivo do jogo não foi encontrado no caminho original.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLaunchDolphinWithCataloguedExecArgument()
    {
        var gamePath = CreateFile("games/RMCP01.wbfs");
        var processRunner = new FakeProcessRunner();
        var history = new FakeEmulatorLaunchHistoryRepository();
        var service = CreateService(
            [CreateGame("RMCP01", gamePath)],
            new FakeExternalToolManager(ExternalToolState.Configured, "/usr/bin/dolphin-emu", "2409"),
            processRunner,
            history);

        var result = await service.ExecuteAsync(new EmulatorLaunchExecutionRequest("RMCP01"));

        result.Started.Should().BeTrue();
        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/usr/bin/dolphin-emu");
        processRunner.LastRequest.Arguments.Should().Equal($"--exec={gamePath}");
        processRunner.LastRequest.WaitForExit.Should().BeFalse();
        history.Entries.Should().ContainSingle().Which.Result.Should().Be("Started");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecordCancelledPreview()
    {
        var gamePath = CreateFile("games/RMCP01.wbfs");
        var history = new FakeEmulatorLaunchHistoryRepository();
        var processRunner = new FakeProcessRunner();
        var service = CreateService(
            [CreateGame("RMCP01", gamePath)],
            new FakeExternalToolManager(ExternalToolState.Configured, "/usr/bin/dolphin-emu", "2409"),
            processRunner,
            history);

        var result = await service.ExecuteAsync(new EmulatorLaunchExecutionRequest("RMCP01", UserConfirmed: false));

        result.Started.Should().BeFalse();
        processRunner.LastRequest.Should().BeNull();
        history.Entries.Should().ContainSingle().Which.Result.Should().Be("Cancelled");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRecordHistoryWhenDisabled()
    {
        var gamePath = CreateFile("games/RMCP01.wbfs");
        var history = new FakeEmulatorLaunchHistoryRepository();
        var service = CreateService(
            [CreateGame("RMCP01", gamePath)],
            new FakeExternalToolManager(ExternalToolState.Configured, "/usr/bin/dolphin-emu", "2409"),
            historyRepository: history);

        await service.ExecuteAsync(new EmulatorLaunchExecutionRequest("RMCP01", SaveHistory: false));

        history.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePreviewAsync_ShouldUseAssignedProfileAndWarnAboutMissingController()
    {
        var gamePath = CreateFile("games/RMCP01.wbfs");
        var profile = new EmulatorProfileDto(
            "wiimote", "dolphin", "Wiimote", false, "Wiimote real", false, "Automático", null,
            new Dictionary<string, string>(), DateTimeOffset.UtcNow);
        var service = CreateService(
            [CreateGame("RMCP01", gamePath)],
            new FakeExternalToolManager(ExternalToolState.Configured, "/usr/bin/dolphin-emu", "2409"),
            profileService: new FakeEmulatorProfileService(profile),
            controllerDetectionService: new FakeControllerDetectionService());

        var preview = await service.CreatePreviewAsync(new EmulatorLaunchRequest("RMCP01"));

        preview.ProfileName.Should().Be("Wiimote");
        preview.ExpectedController.Should().Be("Wiimote real");
        preview.Warnings.Should().ContainSingle().Which.Should().Contain("não foi detectado");
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private EmulatorGameLaunchWorkflowService CreateService(
        IReadOnlyList<GameDto> games,
        IExternalToolManager externalToolManager,
        IProcessRunner? processRunner = null,
        IEmulatorLaunchHistoryRepository? historyRepository = null,
        IEmulatorProfileService? profileService = null,
        IControllerDetectionService? controllerDetectionService = null)
    {
        return new EmulatorGameLaunchWorkflowService(
            new FakeGameLibraryService(games),
            externalToolManager,
            processRunner ?? new FakeProcessRunner(),
            new SafePathService(),
            launchHistoryRepository: historyRepository,
            emulatorProfileService: profileService,
            controllerDetectionService: controllerDetectionService);
    }

    private string CreateFile(string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "game");
        return path;
    }

    private static GameDto CreateGame(string id, string filePath)
    {
        return new GameDto(id, "Mario Kart Wii", Region.Pal, DiscType.Wii, FileFormat.Wbfs, filePath, 1024, DateTimeOffset.UtcNow);
    }

    private sealed class FakeGameLibraryService : IGameLibraryService
    {
        private readonly IReadOnlyList<GameDto> games;

        public FakeGameLibraryService(IReadOnlyList<GameDto> games)
        {
            this.games = games;
        }

        public Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(games);
        }

        public Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExternalToolManager : IExternalToolManager
    {
        private readonly ExternalToolState state;
        private readonly string? executablePath;
        private readonly string? version;

        public FakeExternalToolManager(ExternalToolState state, string? executablePath = null, string? version = null)
        {
            this.state = state;
            this.executablePath = executablePath;
            this.version = version;
        }

        public Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ExternalToolStatus> statuses = [CreateStatus(ExternalToolDefinitionProvider.DolphinToolId)];
            return Task.FromResult(statuses);
        }

        public Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateStatus(toolId));
        }

        public Task<ExternalToolInstallResult> InstallAsync(ExternalToolInstallRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExternalToolInstallResult.Failed(request.ToolId, "not-implemented"));
        }

        public Task<ExternalToolStatus> ConfigureManualPathAsync(ExternalToolManualPathRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateStatus(request.ToolId) with { ExecutablePath = request.ExecutablePath });
        }

        private ExternalToolStatus CreateStatus(string toolId)
        {
            return new ExternalToolStatus(
                toolId,
                "Dolphin Emulator",
                state,
                executablePath,
                version,
                "test",
                state is ExternalToolState.Installed or ExternalToolState.Configured ? null : "externalTools.notInstalled",
                DateTimeOffset.UtcNow);
        }
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public ProcessRequest? LastRequest { get; private set; }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.FromMilliseconds(1)));
        }
    }

    private sealed class FakeEmulatorLaunchHistoryRepository : IEmulatorLaunchHistoryRepository
    {
        public List<EmulatorLaunchHistoryEntry> Entries { get; } = [];

        public Task AppendAsync(EmulatorLaunchHistoryEntry entry, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EmulatorLaunchHistoryEntry>> GetRecentAsync(
            string? gameId = null,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<EmulatorLaunchHistoryEntry> entries = Entries
                .Where(entry => gameId is null || string.Equals(entry.GameId, gameId, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToArray();
            return Task.FromResult(entries);
        }
    }

    private sealed class FakeEmulatorProfileService : IEmulatorProfileService
    {
        private readonly EmulatorProfileDto profile;

        public FakeEmulatorProfileService(EmulatorProfileDto profile) => this.profile = profile;

        public Task<EmulatorProfileDto> ResolveForGameAsync(string gameId, string emulatorId = "dolphin", CancellationToken cancellationToken = default) => Task.FromResult(profile);
        public Task<IReadOnlyList<EmulatorProfileDto>> GetProfilesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<EmulatorProfileDto>>([profile]);
        public Task<EmulatorProfileDto> SaveAsync(EmulatorProfileDto item, CancellationToken cancellationToken = default) => Task.FromResult(item);
        public Task DeleteAsync(string profileId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AssignToGameAsync(string gameId, string profileId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ExportAsync(string profileId, string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<EmulatorProfileDto> ImportAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(profile);
        public Task OpenEmulatorConfigurationAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeControllerDetectionService : IControllerDetectionService
    {
        public Task<ControllerDetectionResult> CheckAsync(string? expectedController, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ControllerDetectionResult(false, "O controle esperado para este perfil não foi detectado."));
    }
}
