using System.IO.Compression;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Security;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Infrastructure.Tests.ExternalTools;

public sealed class ExternalToolManagerTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wiistudio-external-tools-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Locator_ShouldPreferConfiguredPathFromSettings()
    {
        var witPath = CreateExecutable("configured/wit");
        var settings = new FakeSettingsService(new AppSettings { WitExecutablePath = witPath });
        var locator = new ExternalToolLocator(settings, new SafePathService());
        var definition = Definition(ExternalToolDefinitionProvider.WitToolId, "wit", ["missing"]);

        var result = await locator.LocateAsync(definition);

        result.Found.Should().BeTrue();
        result.ExecutablePath.Should().Be(witPath);
        result.DetectionSource.Should().Be("settings");
    }

    [Fact]
    public async Task Locator_ShouldFindToolInToolsDirectory()
    {
        var toolsDirectory = Path.Combine(root, "tools", "wiimms");
        var witPath = CreateExecutable("tools/wiimms/fake-wit-test");
        var locator = new ExternalToolLocator(new FakeSettingsService(new AppSettings()), new SafePathService());
        var definition = Definition(ExternalToolDefinitionProvider.WitToolId, "fake-wit-test", [toolsDirectory]);

        var result = await locator.LocateAsync(definition);

        result.Found.Should().BeTrue();
        result.ExecutablePath.Should().Be(witPath);
        result.DetectionSource.Should().Be(toolsDirectory);
    }

    [Fact]
    public async Task ConfigureManualPathAsync_ShouldValidateAndPersistSettings()
    {
        var witPath = CreateExecutable("manual/wit");
        var settings = new FakeSettingsService(new AppSettings());
        var manager = CreateManager(settings, [Definition(ExternalToolDefinitionProvider.WitToolId, "wit", [])], new FakeProcessRunner("wit: Wiimms ISO Tool v3.05a r8638"));

        var status = await manager.ConfigureManualPathAsync(new ExternalToolManualPathRequest(ExternalToolDefinitionProvider.WitToolId, witPath));

        status.State.Should().Be(ExternalToolState.Configured);
        settings.Current.WitExecutablePath.Should().Be(witPath);
        settings.Current.WitVersion.Should().Be("3.05a");
        settings.Current.ExternalToolsLastCheckAt.Should().NotBeNull();
    }

    [Fact]
    public async Task InstallAsync_ShouldInstallLocalNKitPackageAfterConfirmation()
    {
        var packagePath = CreateNKitPackage("NKit_v1.4.zip");
        var sha = await ComputeSha256Async(packagePath);
        var settings = new FakeSettingsService(new AppSettings());
        var options = new ExternalToolOptions
        {
            ToolsRootDirectory = Path.Combine(root, "tools"),
            PackagesDirectory = Path.GetDirectoryName(packagePath)!,
            TemporaryDirectory = Path.Combine(root, "tmp")
        };
        var definition = NKitDefinition(packagePath, sha, options);
        var manager = CreateManager(settings, [definition], new FakeProcessRunner("NKit 1.4"), options);

        var result = await manager.InstallAsync(new ExternalToolInstallRequest(ExternalToolDefinitionProvider.NKitToolId, UserConfirmed: true));

        result.Success.Should().BeTrue();
        result.ExecutablePath.Should().EndWith("RecoverToISO.exe");
        File.Exists(result.ExecutablePath).Should().BeTrue();
        settings.Current.NKitExecutablePath.Should().Be(result.ExecutablePath);
        settings.Current.NKitVersion.Should().Be("1.4");
    }

    [Fact]
    public async Task InstallAsync_ShouldRequireUserConfirmation()
    {
        var packagePath = CreateNKitPackage("NKit_v1.4.zip");
        var sha = await ComputeSha256Async(packagePath);
        var manager = CreateManager(
            new FakeSettingsService(new AppSettings()),
            [NKitDefinition(packagePath, sha, new ExternalToolOptions { ToolsRootDirectory = Path.Combine(root, "tools"), PackagesDirectory = Path.GetDirectoryName(packagePath)! })],
            new FakeProcessRunner("NKit 1.4"));

        var result = await manager.InstallAsync(new ExternalToolInstallRequest(ExternalToolDefinitionProvider.NKitToolId, UserConfirmed: false));

        result.Success.Should().BeFalse();
        result.ErrorMessageKey.Should().Be("externalTools.install.confirmationRequired");
    }

    [Fact]
    public async Task InstallAsync_ShouldRejectInvalidChecksum()
    {
        var packagePath = CreateNKitPackage("NKit_v1.4.zip");
        var manager = CreateManager(
            new FakeSettingsService(new AppSettings()),
            [NKitDefinition(packagePath, new string('0', 64), new ExternalToolOptions { ToolsRootDirectory = Path.Combine(root, "tools"), PackagesDirectory = Path.GetDirectoryName(packagePath)! })],
            new FakeProcessRunner("NKit 1.4"));

        var result = await manager.InstallAsync(new ExternalToolInstallRequest(ExternalToolDefinitionProvider.NKitToolId, UserConfirmed: true));

        result.Success.Should().BeFalse();
        result.ErrorMessageKey.Should().Be("externalTools.install.checksumInvalid");
    }

    [Fact]
    public async Task ArchiveExtractor_ShouldBlockZipSlip()
    {
        var zipPath = Path.Combine(root, "bad.zip");
        Directory.CreateDirectory(root);
        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("../escape.txt");
        }

        var extractor = new ZipArchiveExtractor();

        var result = await extractor.ExtractAsync(zipPath, Path.Combine(root, "extract"));

        result.Success.Should().BeFalse();
        result.ErrorMessageKey.Should().Be("externalTools.archive.zipSlipBlocked");
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private ExternalToolManager CreateManager(
        FakeSettingsService settings,
        IReadOnlyList<ExternalToolDefinition> definitions,
        IProcessRunner processRunner,
        ExternalToolOptions? options = null)
    {
        options ??= new ExternalToolOptions
        {
            ToolsRootDirectory = Path.Combine(root, "tools"),
            PackagesDirectory = Path.Combine(root, "packages"),
            TemporaryDirectory = Path.Combine(root, "tmp")
        };
        var provider = new FakeDefinitionProvider(definitions);
        var safePath = new SafePathService();
        var installer = new ExternalToolInstaller(
            provider,
            new ExternalToolDownloader(),
            new ChecksumValidator(),
            new ZipArchiveExtractor(),
            options,
            NullLogger<ExternalToolInstaller>.Instance);

        return new ExternalToolManager(
            provider,
            new ExternalToolLocator(settings, safePath),
            installer,
            processRunner,
            settings,
            safePath,
            NullLogger<ExternalToolManager>.Instance);
    }

    private ExternalToolDefinition Definition(string toolId, string executableName, IReadOnlyList<string> searchDirectories) =>
        new(toolId, executableName, [executableName], searchDirectories, "--version", null, IsRequired: true);

    private static ExternalToolDefinition NKitDefinition(string packagePath, string sha, ExternalToolOptions options) =>
        new(
            ExternalToolDefinitionProvider.NKitToolId,
            "NKit Legacy",
            ["RecoverToISO.exe"],
            [Path.Combine(options.ToolsRootDirectory, "nkit", "v1.4")],
            null,
            new ExternalToolPackageSource(
                "local://tools/packages/NKit_v1.4.zip",
                sha,
                "1.4",
                "NKit local test package",
                IsTrusted: true,
                AutoDownloadEnabled: false,
                LocalPackagePath: packagePath),
            IsRequired: false);

    private string CreateExecutable(string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, string.Empty);
        return path;
    }

    private string CreateNKitPackage(string fileName)
    {
        var packagePath = Path.Combine(root, "packages", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(packagePath)!);
        using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
        archive.CreateEntry("RecoverToISO.exe");
        archive.CreateEntry("NKit.dll");
        archive.CreateEntry("Readme.txt");
        return packagePath;
    }

    private static async Task<string> ComputeSha256Async(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await System.Security.Cryptography.SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class FakeDefinitionProvider : IExternalToolDefinitionProvider
    {
        private readonly IReadOnlyList<ExternalToolDefinition> definitions;

        public FakeDefinitionProvider(IReadOnlyList<ExternalToolDefinition> definitions)
        {
            this.definitions = definitions;
        }

        public IReadOnlyList<ExternalToolDefinition> GetDefinitions() => definitions;
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public FakeSettingsService(AppSettings current)
        {
            Current = current;
        }

        public AppSettings Current { get; private set; }

        public Task<AppSettings> GetAsync(CancellationToken cancellationToken) => Task.FromResult(Current);

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(Current);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            Current = settings;
            return Task.CompletedTask;
        }

        public Task ResetAsync(CancellationToken cancellationToken)
        {
            Current = new AppSettings();
            return Task.CompletedTask;
        }

        public Task ExportAsync(string filePath, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ImportAsync(string filePath, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        private readonly string standardOutput;

        public FakeProcessRunner(string standardOutput)
        {
            this.standardOutput = standardOutput;
        }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ProcessResult(0, standardOutput, string.Empty, TimeSpan.FromMilliseconds(1)));
        }
    }
}
