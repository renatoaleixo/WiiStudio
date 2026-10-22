using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Emulators;
using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Tests.Emulators;

public sealed class EmulatorCompatibilityAndSaveServicesTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wiistudio-save-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task CompatibilityService_ShouldReturnNotTestedAndPersistNotes()
    {
        var repository = new FakeCompatibilityRepository();
        var service = new EmulatorCompatibilityService(repository);

        var initial = await service.GetAsync("RMCP01");
        var saved = await service.SaveAsync(initial with { Status = LocalCompatibilityStatus.WorksWell, Notes = "Funciona a 60 FPS" });

        initial.Status.Should().Be(LocalCompatibilityStatus.NotTested);
        saved.Notes.Should().Be("Funciona a 60 FPS");
        (await service.GetAsync("RMCP01")).Status.Should().Be(LocalCompatibilityStatus.WorksWell);
    }

    [Fact]
    public async Task LocateSavesAsync_ShouldFindValidatedWiiTitleDirectory()
    {
        var saveDirectory = CreateSaveDirectory();
        var service = CreateSaveService();

        var result = await service.LocateSavesAsync("RMCP01");

        result.Found.Should().BeTrue();
        result.SavePath.Should().Be(saveDirectory);
    }

    [Fact]
    public async Task LocateSavesAsync_ShouldRequireConfiguredDolphin()
    {
        var service = CreateSaveService(ExternalToolState.NotInstalled);

        var act = () => service.LocateSavesAsync("RMCP01");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Configure o Dolphin*");
    }

    [Fact]
    public async Task BackupSavesAsync_ShouldRequireConfirmationAndPreserveOriginals()
    {
        var saveDirectory = CreateSaveDirectory();
        var original = Path.Combine(saveDirectory, "banner.bin");
        var destination = Path.Combine(root, "backups", "RMCP01.zip");
        var repository = new FakeSaveBackupRepository();
        var service = CreateSaveService(repository: repository);

        var cancelled = await service.BackupSavesAsync(new SaveBackupRequest("RMCP01", "dolphin", destination, false));
        var completed = await service.BackupSavesAsync(new SaveBackupRequest("RMCP01", "dolphin", destination, true));

        cancelled.Success.Should().BeFalse();
        completed.Success.Should().BeTrue();
        File.Exists(destination).Should().BeTrue();
        File.Exists(original).Should().BeTrue();
        repository.Entries.Should().ContainSingle(entry => entry.Success);
    }

    [Fact]
    public async Task BackupSavesAsync_ShouldBlockDestinationInsideSaveDirectory()
    {
        var saveDirectory = CreateSaveDirectory();
        var service = CreateSaveService();

        var act = () => service.BackupSavesAsync(new SaveBackupRequest("RMCP01", "dolphin", Path.Combine(saveDirectory, "backup.zip"), true));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*pasta original*");
    }

    [Fact]
    public async Task OpenSavesFolderAsync_ShouldUsePlatformOpenerAfterConfirmation()
    {
        CreateSaveDirectory();
        var runner = new FakeProcessRunner();
        var service = CreateSaveService(runner: runner);

        await service.OpenSavesFolderAsync("RMCP01", true);

        runner.LastRequest.Should().NotBeNull();
        runner.LastRequest!.WaitForExit.Should().BeFalse();
        runner.LastRequest.Arguments.Should().ContainSingle();
    }

    private EmulatorSaveManagementService CreateSaveService(
        ExternalToolState state = ExternalToolState.Configured,
        FakeSaveBackupRepository? repository = null,
        IProcessRunner? runner = null)
    {
        Directory.CreateDirectory(root);
        return new EmulatorSaveManagementService(
            new EmulatorSaveOptions { UserDirectoryRoots = [root], BackupDirectory = Path.Combine(root, "backups") },
            new FakeExternalToolManager(state),
            runner ?? new FakeProcessRunner(),
            repository ?? new FakeSaveBackupRepository());
    }

    private string CreateSaveDirectory()
    {
        var directory = Path.Combine(root, "Wii", "title", "00010000", "524d4350", "data");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "banner.bin"), "save-data");
        return directory;
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private sealed class FakeCompatibilityRepository : IEmulatorCompatibilityRepository
    {
        private EmulatorCompatibilityDto? item;
        public Task<EmulatorCompatibilityDto?> GetAsync(string gameId, string emulatorId, CancellationToken cancellationToken = default) => Task.FromResult(item);
        public Task UpsertAsync(EmulatorCompatibilityDto compatibility, CancellationToken cancellationToken = default) { item = compatibility; return Task.CompletedTask; }
    }

    private sealed class FakeSaveBackupRepository : ISaveBackupRepository
    {
        public List<SaveBackupEntry> Entries { get; } = [];
        public Task AppendAsync(SaveBackupEntry entry, CancellationToken cancellationToken = default) { Entries.Add(entry); return Task.CompletedTask; }
        public Task<IReadOnlyList<SaveBackupEntry>> GetRecentAsync(string? gameId = null, int limit = 100, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SaveBackupEntry>>(Entries);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public ProcessRequest? LastRequest { get; private set; }
        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken) { LastRequest = request; return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.Zero)); }
    }

    private sealed class FakeExternalToolManager : IExternalToolManager
    {
        private readonly ExternalToolState state;
        public FakeExternalToolManager(ExternalToolState state) => this.state = state;
        public Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default) => Task.FromResult(new ExternalToolStatus(toolId, "Dolphin", state, state == ExternalToolState.Configured ? "/usr/bin/dolphin" : null, "2506", "test", null, DateTimeOffset.UtcNow));
        public Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ExternalToolStatus>>([]);
        public Task<ExternalToolInstallResult> InstallAsync(ExternalToolInstallRequest request, CancellationToken cancellationToken = default) => Task.FromResult(ExternalToolInstallResult.Failed(request.ToolId, "not-used"));
        public Task<ExternalToolStatus> ConfigureManualPathAsync(ExternalToolManualPathRequest request, CancellationToken cancellationToken = default) => CheckAsync(request.ToolId, cancellationToken);
    }
}
