using System.Text.Json;
using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Settings;
using WiiStudio.Infrastructure.Themes;

namespace WiiStudio.Infrastructure.Tests.Themes;

public sealed class JsonThemeServiceTests
{
    [Fact]
    public async Task GetThemesAsync_ShouldReturnBuiltInThemes()
    {
        using var fixture = TemporaryThemeFixture.Create();
        var service = fixture.CreateService();

        var themes = await service.GetThemesAsync(CancellationToken.None);

        themes.Select(theme => theme.Id).Should().Contain(["System", "Light", "Dark"]);
        themes.Single(theme => theme.Id == "System").IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyThemeAsync_ShouldPersistSelectedTheme()
    {
        using var fixture = TemporaryThemeFixture.Create();
        var service = fixture.CreateService();

        var snapshot = await service.ApplyThemeAsync("Dark", CancellationToken.None);

        snapshot.ActiveTheme.Id.Should().Be("Dark");
        var settings = await fixture.CreateSettingsService().GetAsync(CancellationToken.None);
        settings.Theme.Should().Be("Dark");
    }

    [Fact]
    public async Task ImportThemeAsync_ShouldPersistCustomTheme()
    {
        using var fixture = TemporaryThemeFixture.Create();
        var importPath = Path.Combine(fixture.RootDirectory, "my-theme.json");
        await WriteThemeAsync(importPath, new ThemeInfo("Ocean", "Ocean", ThemeKind.Custom, IsBuiltIn: false, IsActive: false));
        var service = fixture.CreateService();

        var imported = await service.ImportThemeAsync(importPath, CancellationToken.None);

        imported.Id.Should().Be("Ocean");
        File.Exists(Path.Combine(fixture.ThemeDirectoryPath, "Ocean.theme.json")).Should().BeTrue();
        var themes = await service.GetThemesAsync(CancellationToken.None);
        themes.Should().Contain(theme => theme.Id == "Ocean");
    }

    [Fact]
    public async Task ImportThemeAsync_ShouldRejectBuiltInThemeId()
    {
        using var fixture = TemporaryThemeFixture.Create();
        var importPath = Path.Combine(fixture.RootDirectory, "dark.json");
        await WriteThemeAsync(importPath, new ThemeInfo("Dark", "Dark copy", ThemeKind.Custom, IsBuiltIn: false, IsActive: false));
        var service = fixture.CreateService();

        var act = () => service.ImportThemeAsync(importPath, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExportThemeAsync_ShouldWriteThemeFile()
    {
        using var fixture = TemporaryThemeFixture.Create();
        var service = fixture.CreateService();
        var exportPath = Path.Combine(fixture.RootDirectory, "exported-theme.json");

        await service.ExportThemeAsync("Light", exportPath, CancellationToken.None);

        File.Exists(exportPath).Should().BeTrue();
        await using var stream = File.OpenRead(exportPath);
        var exported = await JsonSerializer.DeserializeAsync<ThemeInfo>(stream);
        exported!.Id.Should().Be("Light");
        exported.IsActive.Should().BeFalse();
    }

    private static async Task WriteThemeAsync(string filePath, ThemeInfo theme)
    {
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, theme, new JsonSerializerOptions { WriteIndented = true });
    }

    private sealed class TemporaryThemeFixture : IDisposable
    {
        private TemporaryThemeFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            SettingsPath = Path.Combine(rootDirectory, "settings.json");
            ThemeDirectoryPath = Path.Combine(rootDirectory, "themes");
            Directory.CreateDirectory(rootDirectory);
        }

        public string RootDirectory { get; }

        public string SettingsPath { get; }

        public string ThemeDirectoryPath { get; }

        public static TemporaryThemeFixture Create()
        {
            return new TemporaryThemeFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-themes-{Guid.NewGuid():N}"));
        }

        public JsonSettingsService CreateSettingsService()
        {
            return new JsonSettingsService(new SettingsStoreOptions { FilePath = SettingsPath });
        }

        public JsonThemeService CreateService()
        {
            return new JsonThemeService(
                CreateSettingsService(),
                new ThemeStoreOptions { ThemeDirectoryPath = ThemeDirectoryPath });
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
