using FluentAssertions;
using WiiStudio.Infrastructure.Settings;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Infrastructure.Tests.Settings;

public sealed class JsonSettingsServiceTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnDefaultSettingsWhenFileDoesNotExist()
    {
        using var fixture = TemporarySettingsFixture.Create();
        var service = fixture.CreateService();

        var settings = await service.LoadAsync(CancellationToken.None);

        settings.Language.Should().Be("pt-BR");
        settings.Theme.Should().Be("System");
        settings.MaxParallelTasks.Should().Be(2);
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistSettings()
    {
        using var fixture = TemporarySettingsFixture.Create();
        var service = fixture.CreateService();
        var settings = new AppSettings
        {
            Language = "en-US",
            Theme = "Dark",
            LibraryPath = "/games",
            MaxParallelTasks = 4
        };

        await service.SaveAsync(settings, CancellationToken.None);
        var loaded = await service.LoadAsync(CancellationToken.None);

        loaded.Language.Should().Be("en-US");
        loaded.Theme.Should().Be("Dark");
        loaded.LibraryPath.Should().Be("/games");
        loaded.MaxParallelTasks.Should().Be(4);
    }

    [Fact]
    public async Task GetAsync_ShouldLoadSettings()
    {
        using var fixture = TemporarySettingsFixture.Create();
        var service = fixture.CreateService();
        await service.SaveAsync(new AppSettings { Language = "en-US" }, CancellationToken.None);

        var settings = await service.GetAsync(CancellationToken.None);

        settings.Language.Should().Be("en-US");
    }

    [Fact]
    public async Task ResetAsync_ShouldRestoreDefaults()
    {
        using var fixture = TemporarySettingsFixture.Create();
        var service = fixture.CreateService();
        await service.SaveAsync(new AppSettings { Language = "en-US", Theme = "Dark", MaxParallelTasks = 4 }, CancellationToken.None);

        await service.ResetAsync(CancellationToken.None);

        var settings = await service.LoadAsync(CancellationToken.None);
        settings.Language.Should().Be("pt-BR");
        settings.Theme.Should().Be("System");
        settings.MaxParallelTasks.Should().Be(2);
    }

    [Fact]
    public async Task ExportAsync_ShouldWriteCurrentSettingsToDestination()
    {
        using var fixture = TemporarySettingsFixture.Create();
        var service = fixture.CreateService();
        var exportPath = Path.Combine(fixture.RootDirectory, "export", "settings.json");
        await service.SaveAsync(new AppSettings { Language = "en-US", Theme = "Dark" }, CancellationToken.None);

        await service.ExportAsync(exportPath, CancellationToken.None);

        File.Exists(exportPath).Should().BeTrue();
        var exported = await File.ReadAllTextAsync(exportPath);
        exported.Should().Contain("en-US");
        exported.Should().Contain("Dark");
    }

    [Fact]
    public async Task ImportAsync_ShouldBackupCurrentSettingsAndPersistImportedSettings()
    {
        using var fixture = TemporarySettingsFixture.Create();
        var service = fixture.CreateService();
        var importPath = Path.Combine(fixture.RootDirectory, "import.json");
        await service.SaveAsync(new AppSettings { Language = "pt-BR", Theme = "System" }, CancellationToken.None);
        await service.ExportAsync(importPath, CancellationToken.None);
        await service.SaveAsync(new AppSettings { Language = "en-US", Theme = "Dark" }, CancellationToken.None);

        await service.ImportAsync(importPath, CancellationToken.None);

        var imported = await service.LoadAsync(CancellationToken.None);
        imported.Language.Should().Be("pt-BR");
        imported.Theme.Should().Be("System");
        File.Exists($"{fixture.SettingsPath}.bak").Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_ShouldRejectInvalidSettings()
    {
        using var fixture = TemporarySettingsFixture.Create();
        var service = fixture.CreateService();

        var act = () => service.SaveAsync(new AppSettings { MaxParallelTasks = 0 }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class TemporarySettingsFixture : IDisposable
    {
        private TemporarySettingsFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            SettingsPath = Path.Combine(rootDirectory, "config", "settings.json");
        }

        public string RootDirectory { get; }

        public string SettingsPath { get; }

        public static TemporarySettingsFixture Create()
        {
            return new TemporarySettingsFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-settings-{Guid.NewGuid():N}"));
        }

        public JsonSettingsService CreateService()
        {
            return new JsonSettingsService(new SettingsStoreOptions { FilePath = SettingsPath });
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
