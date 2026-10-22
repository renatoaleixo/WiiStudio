using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Emulators;
using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Tests.Emulators;

public sealed class EmulatorProfileServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wiistudio-profile-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task GetProfilesAsync_ShouldCreateDefaultProfile()
    {
        var repository = new FakeRepository();
        var service = CreateService(repository);

        var profiles = await service.GetProfilesAsync();

        profiles.Should().ContainSingle();
        profiles[0].ProfileId.Should().Be(EmulatorProfileService.DefaultProfileId);
        profiles[0].IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAndAssign_ShouldResolveGameProfileOverDefault()
    {
        var repository = new FakeRepository();
        var service = CreateService(repository);
        await service.GetProfilesAsync();
        var saved = await service.SaveAsync(Profile(name: "Wiimote", controller: "Wiimote real"));

        await service.AssignToGameAsync("rmcp01", saved.ProfileId);
        var resolved = await service.ResolveForGameAsync("RMCP01");

        resolved.ProfileId.Should().Be(saved.ProfileId);
        resolved.ExpectedController.Should().Be("Wiimote real");
    }

    [Fact]
    public async Task ResolveForGameAsync_ShouldUseGlobalDefaultWithoutAssignment()
    {
        var service = CreateService(new FakeRepository());

        var resolved = await service.ResolveForGameAsync("RMCP01");

        resolved.IsDefault.Should().BeTrue();
        resolved.Name.Should().Be("Padrão");
    }

    [Fact]
    public async Task DeleteAsync_ShouldProtectDefaultAndRemoveNonDefault()
    {
        var repository = new FakeRepository();
        var service = CreateService(repository);
        var defaultProfile = (await service.GetProfilesAsync()).Single();
        var custom = await service.SaveAsync(Profile("Performance"));

        var act = () => service.DeleteAsync(defaultProfile.ProfileId);
        await act.Should().ThrowAsync<InvalidOperationException>();
        await service.DeleteAsync(custom.ProfileId);

        (await service.GetProfilesAsync()).Should().ContainSingle(profile => profile.IsDefault);
    }

    [Fact]
    public async Task ExportAndImport_ShouldRoundTripAsNewNonDefaultProfile()
    {
        Directory.CreateDirectory(root);
        var service = CreateService(new FakeRepository());
        var saved = await service.SaveAsync(Profile("Sala"));
        var path = Path.Combine(root, "perfil.json");

        await service.ExportAsync(saved.ProfileId, path);
        var imported = await service.ImportAsync(path);

        imported.Name.Should().Be("Sala");
        imported.ProfileId.Should().NotBe(saved.ProfileId);
        imported.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task OpenEmulatorConfigurationAsync_ShouldStartDolphinWithoutArguments()
    {
        var runner = new FakeProcessRunner();
        var service = CreateService(new FakeRepository(), runner);

        await service.OpenEmulatorConfigurationAsync();

        runner.LastRequest.Should().NotBeNull();
        runner.LastRequest!.ExecutablePath.Should().Be("/usr/bin/dolphin-emu");
        runner.LastRequest.Arguments.Should().BeEmpty();
        runner.LastRequest.WaitForExit.Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static EmulatorProfileDto Profile(string name, string controller = "Nenhum") =>
        new(string.Empty, "dolphin", name, false, controller, false, "Automático", null, new Dictionary<string, string>(), DateTimeOffset.UtcNow);

    private static EmulatorProfileService CreateService(FakeRepository repository, IProcessRunner? runner = null) =>
        new(repository, new FakeExternalToolManager(), runner ?? new FakeProcessRunner());

    private sealed class FakeRepository : IEmulatorProfileRepository
    {
        private readonly Dictionary<string, EmulatorProfileDto> profiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameEmulatorProfileAssignmentDto> assignments = new(StringComparer.OrdinalIgnoreCase);

        public Task<IReadOnlyList<EmulatorProfileDto>> GetProfilesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmulatorProfileDto>>(profiles.Values.OrderByDescending(profile => profile.IsDefault).ToArray());

        public Task<EmulatorProfileDto?> GetByIdAsync(string profileId, CancellationToken cancellationToken = default) =>
            Task.FromResult(profiles.GetValueOrDefault(profileId));

        public Task UpsertAsync(EmulatorProfileDto profile, CancellationToken cancellationToken = default)
        {
            if (profile.IsDefault)
            {
                foreach (var current in profiles.Values.Where(item => item.EmulatorId == profile.EmulatorId).ToArray())
                {
                    profiles[current.ProfileId] = current with { IsDefault = false };
                }
            }

            profiles[profile.ProfileId] = profile;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string profileId, CancellationToken cancellationToken = default)
        {
            profiles.Remove(profileId);
            return Task.CompletedTask;
        }

        public Task<GameEmulatorProfileAssignmentDto?> GetAssignmentAsync(string gameId, string emulatorId, CancellationToken cancellationToken = default) =>
            Task.FromResult(assignments.GetValueOrDefault($"{gameId}:{emulatorId}"));

        public Task UpsertAssignmentAsync(GameEmulatorProfileAssignmentDto assignment, CancellationToken cancellationToken = default)
        {
            assignments[$"{assignment.GameId}:{assignment.EmulatorId}"] = assignment;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExternalToolManager : IExternalToolManager
    {
        public Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ExternalToolStatus(toolId, "Dolphin", ExternalToolState.Configured, "/usr/bin/dolphin-emu", "2506", "test", null, DateTimeOffset.UtcNow));

        public Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ExternalToolStatus>>([]);

        public Task<ExternalToolInstallResult> InstallAsync(ExternalToolInstallRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExternalToolInstallResult.Failed(request.ToolId, "not-used"));

        public Task<ExternalToolStatus> ConfigureManualPathAsync(ExternalToolManualPathRequest request, CancellationToken cancellationToken = default) =>
            CheckAsync(request.ToolId, cancellationToken);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public ProcessRequest? LastRequest { get; private set; }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.Zero));
        }
    }
}
