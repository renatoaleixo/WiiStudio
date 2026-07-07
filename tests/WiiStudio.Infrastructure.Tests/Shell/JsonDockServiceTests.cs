using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Shell;

namespace WiiStudio.Infrastructure.Tests.Shell;

public sealed class JsonDockServiceTests
{
    [Fact]
    public async Task GetLayoutsAsync_ShouldReturnDefaultWhenDirectoryDoesNotExist()
    {
        using var fixture = TemporaryDockFixture.Create();
        var service = fixture.CreateService();

        var layouts = await service.GetLayoutsAsync(CancellationToken.None);

        layouts.Should().ContainSingle().Which.Should().Be("Default");
    }

    [Fact]
    public async Task SaveLayoutAsync_ShouldPersistNamedLayout()
    {
        using var fixture = TemporaryDockFixture.Create();
        var service = fixture.CreateService();

        await service.SaveLayoutAsync("Development", CancellationToken.None);

        File.Exists(Path.Combine(fixture.LayoutDirectoryPath, "Development.layout.json")).Should().BeTrue();
        var layouts = await service.GetLayoutsAsync(CancellationToken.None);
        layouts.Should().Contain(["Default", "Development"]);
    }

    [Fact]
    public async Task LoadLayoutAsync_ShouldRestoreSavedLayout()
    {
        using var fixture = TemporaryDockFixture.Create();
        var service = fixture.CreateService();
        await service.SaveLayoutAsync("Gaming", CancellationToken.None);

        var restored = fixture.CreateService();
        await restored.LoadLayoutAsync("Gaming", CancellationToken.None);

        restored.CurrentLayout.Name.Should().Be("Gaming");
        restored.CurrentLayout.Panels.Should().Contain(panel => panel.Id == "library");
        restored.CurrentLayout.State.Should().Be(DockLayoutState.Ready);
    }

    [Fact]
    public async Task ResetLayoutAsync_ShouldPersistDefaultLayout()
    {
        using var fixture = TemporaryDockFixture.Create();
        var service = fixture.CreateService();

        await service.ResetLayoutAsync(CancellationToken.None);

        service.CurrentLayout.Name.Should().Be("Default");
        File.Exists(Path.Combine(fixture.LayoutDirectoryPath, "Default.layout.json")).Should().BeTrue();
    }

    [Fact]
    public async Task LoadLayoutAsync_ShouldThrowWhenNamedLayoutIsMissing()
    {
        using var fixture = TemporaryDockFixture.Create();
        var service = fixture.CreateService();

        var act = () => service.LoadLayoutAsync("Missing", CancellationToken.None);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    private sealed class TemporaryDockFixture : IDisposable
    {
        private TemporaryDockFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            LayoutDirectoryPath = Path.Combine(rootDirectory, "profiles", "layouts");
        }

        public string RootDirectory { get; }

        public string LayoutDirectoryPath { get; }

        public static TemporaryDockFixture Create()
        {
            return new TemporaryDockFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-dock-{Guid.NewGuid():N}"));
        }

        public JsonDockService CreateService()
        {
            return new JsonDockService(new DockLayoutStoreOptions { LayoutDirectoryPath = LayoutDirectoryPath });
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
