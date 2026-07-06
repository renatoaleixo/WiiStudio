using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Shell;

namespace WiiStudio.Infrastructure.Tests.Shell;

public sealed class JsonMainWindowServiceTests
{
    [Fact]
    public async Task InitializeAsync_ShouldReturnDefaultLayoutAndModulesWhenNoSavedLayoutExists()
    {
        using var fixture = TemporaryShellFixture.Create();
        var service = fixture.CreateService();

        var snapshot = await service.InitializeAsync(CancellationToken.None);

        snapshot.State.Should().Be(MainWindowState.Ready);
        snapshot.Layout.ActiveModuleId.Should().Be(ShellModuleId.Dashboard);
        snapshot.Modules.Should().Contain(module => module.Id == ShellModuleId.Library);
    }

    [Fact]
    public async Task SaveLayoutAsync_ShouldPersistSelectedModuleAndSidePanelState()
    {
        using var fixture = TemporaryShellFixture.Create();
        var service = fixture.CreateService();
        await service.SelectModuleAsync(ShellModuleId.Diagnostics, CancellationToken.None);
        await service.SetSidePanelVisibilityAsync(false, CancellationToken.None);

        await service.SaveLayoutAsync(CancellationToken.None);
        var restored = await fixture.CreateService().RestoreLayoutAsync(CancellationToken.None);

        restored.ActiveModuleId.Should().Be(ShellModuleId.Diagnostics);
        restored.IsSidePanelVisible.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreLayoutAsync_ShouldNormalizeInvalidDimensions()
    {
        using var fixture = TemporaryShellFixture.Create();
        await File.WriteAllTextAsync(fixture.LayoutPath, """
            {
              "activeModuleId": 2,
              "isSidePanelVisible": true,
              "navigationWidth": 10,
              "sidePanelWidth": 900
            }
            """);

        var layout = await fixture.CreateService().RestoreLayoutAsync(CancellationToken.None);

        layout.NavigationWidth.Should().Be(180);
        layout.SidePanelWidth.Should().Be(480);
    }

    [Fact]
    public async Task SelectModuleAsync_ShouldRejectUnavailableModule()
    {
        using var fixture = TemporaryShellFixture.Create();
        var service = fixture.CreateService();

        var act = () => service.SelectModuleAsync((ShellModuleId)999, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private sealed class TemporaryShellFixture : IDisposable
    {
        private TemporaryShellFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            LayoutPath = Path.Combine(rootDirectory, "main-window-layout.json");
            Directory.CreateDirectory(rootDirectory);
        }

        public string RootDirectory { get; }

        public string LayoutPath { get; }

        public static TemporaryShellFixture Create()
        {
            return new TemporaryShellFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-shell-{Guid.NewGuid():N}"));
        }

        public JsonMainWindowService CreateService()
        {
            return new JsonMainWindowService(new MainWindowStoreOptions { LayoutFilePath = LayoutPath });
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }
}
