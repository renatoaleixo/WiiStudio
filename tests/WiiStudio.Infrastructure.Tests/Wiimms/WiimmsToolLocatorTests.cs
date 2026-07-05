using FluentAssertions;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Wiimms;

public sealed class WiimmsToolLocatorTests
{
    [Fact]
    public async Task FindWitAsync_ShouldPreferConfiguredPath()
    {
        using var fixture = TemporaryWiimmsFixture.Create();
        var configuredWit = fixture.CreateTool("configured", "wit");
        fixture.CreateTool("path", "wit");

        var locator = new WiimmsToolLocator(new WiimmsToolOptions
        {
            WitExecutablePath = configuredWit,
            PathEnvironment = fixture.PathDirectory,
            ToolsDirectory = fixture.ToolsDirectory,
            ApplicationDirectory = fixture.ApplicationDirectory
        });

        var witPath = await locator.FindWitAsync(CancellationToken.None);

        witPath.Should().Be(configuredWit);
    }

    [Fact]
    public async Task FindWitAsync_ShouldSearchPathBeforeToolsAndApplicationDirectories()
    {
        using var fixture = TemporaryWiimmsFixture.Create();
        var pathWit = fixture.CreateTool("path", "wit");
        fixture.CreateTool("tools", "wit");
        fixture.CreateTool("app", "wit");

        var locator = new WiimmsToolLocator(new WiimmsToolOptions
        {
            PathEnvironment = fixture.PathDirectory,
            ToolsDirectory = fixture.ToolsDirectory,
            ApplicationDirectory = fixture.ApplicationDirectory
        });

        var witPath = await locator.FindWitAsync(CancellationToken.None);

        witPath.Should().Be(pathWit);
    }

    [Fact]
    public async Task FindWitAsync_ShouldSearchToolsBeforeApplicationDirectory()
    {
        using var fixture = TemporaryWiimmsFixture.Create();
        var toolsWit = fixture.CreateTool("tools", "wit");
        fixture.CreateTool("app", "wit");

        var locator = new WiimmsToolLocator(new WiimmsToolOptions
        {
            PathEnvironment = string.Empty,
            ToolsDirectory = fixture.ToolsDirectory,
            ApplicationDirectory = fixture.ApplicationDirectory
        });

        var witPath = await locator.FindWitAsync(CancellationToken.None);

        witPath.Should().Be(toolsWit);
    }

    [Fact]
    public async Task FindWwtAsync_ShouldFindWwtIndependentlyFromWit()
    {
        using var fixture = TemporaryWiimmsFixture.Create();
        var wwtPath = fixture.CreateTool("tools", "wwt");

        var locator = new WiimmsToolLocator(new WiimmsToolOptions
        {
            PathEnvironment = string.Empty,
            ToolsDirectory = fixture.ToolsDirectory,
            ApplicationDirectory = fixture.ApplicationDirectory
        });

        var foundPath = await locator.FindWwtAsync(CancellationToken.None);

        foundPath.Should().Be(wwtPath);
    }

    [Fact]
    public async Task FindWitAsync_ShouldThrowWhenToolIsMissing()
    {
        using var fixture = TemporaryWiimmsFixture.Create();
        var locator = new WiimmsToolLocator(new WiimmsToolOptions
        {
            PathEnvironment = string.Empty,
            ToolsDirectory = fixture.ToolsDirectory,
            ApplicationDirectory = fixture.ApplicationDirectory
        });

        var act = () => locator.FindWitAsync(CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsNotInstalledException>();
        exception.Which.ToolName.Should().Be("wit");
    }

    private sealed class TemporaryWiimmsFixture : IDisposable
    {
        private TemporaryWiimmsFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            ConfiguredDirectory = Directory.CreateDirectory(Path.Combine(rootDirectory, "configured")).FullName;
            PathDirectory = Directory.CreateDirectory(Path.Combine(rootDirectory, "path")).FullName;
            ToolsDirectory = Directory.CreateDirectory(Path.Combine(rootDirectory, "tools")).FullName;
            ApplicationDirectory = Directory.CreateDirectory(Path.Combine(rootDirectory, "app")).FullName;
        }

        public string RootDirectory { get; }

        public string ConfiguredDirectory { get; }

        public string PathDirectory { get; }

        public string ToolsDirectory { get; }

        public string ApplicationDirectory { get; }

        public static TemporaryWiimmsFixture Create()
        {
            var rootDirectory = Path.Combine(Path.GetTempPath(), $"wiistudio-wiimms-{Guid.NewGuid():N}");

            return new TemporaryWiimmsFixture(rootDirectory);
        }

        public string CreateTool(string directoryName, string toolName)
        {
            var directory = directoryName switch
            {
                "configured" => ConfiguredDirectory,
                "path" => PathDirectory,
                "tools" => ToolsDirectory,
                "app" => ApplicationDirectory,
                _ => throw new ArgumentOutOfRangeException(nameof(directoryName), directoryName, "Unknown tool directory.")
            };

            var toolPath = Path.Combine(directory, toolName);
            File.WriteAllText(toolPath, string.Empty);

            return Path.GetFullPath(toolPath);
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
