using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Security;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Infrastructure.ExternalTools;

public sealed class ExternalToolLocator : IExternalToolLocator
{
    private readonly ISettingsService settingsService;
    private readonly ISafePathService safePathService;

    public ExternalToolLocator(ISettingsService settingsService, ISafePathService safePathService)
    {
        this.settingsService = settingsService;
        this.safePathService = safePathService;
    }

    public async Task<ExternalToolLocationResult> LocateAsync(
        ExternalToolDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        cancellationToken.ThrowIfCancellationRequested();

        var settings = await settingsService.GetAsync(cancellationToken);
        var configuredPath = GetConfiguredPath(settings, definition.ToolId);
        var configured = FindConfiguredPath(configuredPath);
        if (configured is not null)
        {
            return ExternalToolLocationResult.FoundAt(configured, "settings");
        }

        var pathResult = FindInPath(definition.ExecutableNames);
        if (pathResult is not null)
        {
            return ExternalToolLocationResult.FoundAt(pathResult, "path");
        }

        foreach (var directory in definition.SearchDirectories)
        {
            var found = FindInDirectory(directory, definition.ExecutableNames);
            if (found is not null)
            {
                return ExternalToolLocationResult.FoundAt(found, directory);
            }
        }

        return ExternalToolLocationResult.Missing();
    }

    private string? FindConfiguredPath(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return null;
        }

        var result = safePathService.ValidateReadableFile(configuredPath);
        return result.Success ? result.NormalizedPath : null;
    }

    private static string? FindInPath(IReadOnlyList<string> executableNames)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var found = FindInDirectory(directory, executableNames);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static string? FindInDirectory(string directory, IReadOnlyList<string> executableNames)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return null;
        }

        foreach (var executableName in executableNames)
        {
            var candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
    }

    private static string? GetConfiguredPath(AppSettings settings, string toolId)
    {
        return toolId switch
        {
            ExternalToolDefinitionProvider.WitToolId => settings.WitExecutablePath,
            ExternalToolDefinitionProvider.WwtToolId => settings.WwtExecutablePath,
            ExternalToolDefinitionProvider.NKitToolId => settings.NKitExecutablePath,
            _ => null
        };
    }
}

public sealed class ChecksumValidator : IChecksumValidator
{
    public async Task<ChecksumValidationResult> ValidateSha256Async(
        string filePath,
        string expectedSha256,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedSha256);

        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        var actual = Convert.ToHexString(hash).ToLowerInvariant();
        var expected = expectedSha256.ToLowerInvariant();

        return new ChecksumValidationResult(
            string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            actual,
            expected);
    }
}

public sealed class ZipArchiveExtractor : IArchiveExtractor
{
    public Task<ArchiveExtractionResult> ExtractAsync(
        string archivePath,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(destinationDirectory);
        var destinationRoot = EnsureTrailingSeparator(Path.GetFullPath(destinationDirectory));
        var extracted = new List<string>();

        using var archive = ZipFile.OpenRead(archivePath);
        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(entry.FullName))
            {
                continue;
            }

            var targetPath = Path.GetFullPath(Path.Combine(destinationRoot, entry.FullName));
            if (!targetPath.StartsWith(destinationRoot, PathComparison))
            {
                return Task.FromResult(ArchiveExtractionResult.Failed(destinationDirectory, "externalTools.archive.zipSlipBlocked"));
            }

            if (entry.FullName.EndsWith("/", StringComparison.Ordinal))
            {
                Directory.CreateDirectory(targetPath);
                continue;
            }

            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            entry.ExtractToFile(targetPath, overwrite: true);
            extracted.Add(targetPath);
        }

        return Task.FromResult(new ArchiveExtractionResult(true, destinationDirectory, extracted, null));
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
}

public sealed class ExternalToolDownloader : IExternalToolDownloader
{
    private readonly IExternalToolNetworkDownloader? networkDownloader;

    public ExternalToolDownloader(IExternalToolNetworkDownloader? networkDownloader = null)
    {
        this.networkDownloader = networkDownloader;
    }

    public Task<DownloadedToolPackage> DownloadAsync(
        ExternalToolPackageSource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrWhiteSpace(source.LocalPackagePath))
        {
            if (!File.Exists(source.LocalPackagePath))
            {
                throw new FileNotFoundException("Local tool package was not found.", source.LocalPackagePath);
            }

            return Task.FromResult(new DownloadedToolPackage(source.LocalPackagePath, source));
        }

        if (!source.AutoDownloadEnabled)
        {
            throw new InvalidOperationException("Automatic download is disabled for this tool package.");
        }

        if (networkDownloader is null)
        {
            throw new NotSupportedException("Network downloads are not configured in the default pipeline.");
        }

        return networkDownloader.DownloadAsync(source, progress: null, cancellationToken);
    }
}

public sealed class ExternalToolNetworkDownloader : IExternalToolNetworkDownloader
{
    private readonly HttpClient httpClient;
    private readonly IChecksumValidator checksumValidator;
    private readonly ExternalToolOptions options;

    public ExternalToolNetworkDownloader(
        HttpClient httpClient,
        IChecksumValidator checksumValidator,
        ExternalToolOptions options)
    {
        this.httpClient = httpClient;
        this.checksumValidator = checksumValidator;
        this.options = options;
    }

    public async Task<DownloadedToolPackage> DownloadAsync(
        ExternalToolPackageSource source,
        IProgress<ExternalToolDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateSource(source);
        Directory.CreateDirectory(options.TemporaryDirectory);

        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(options.DownloadTimeoutSeconds));
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        var uri = new Uri(source.DownloadUrl);
        var fileName = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"{source.SourceName}-{source.Version}.zip";
        }

        var targetPath = Path.Combine(options.TemporaryDirectory, $"{Guid.NewGuid():N}-{fileName}");
        try
        {
            progress?.Report(new ExternalToolDownloadProgress(null, 0, null, "externalTools.download.starting", ExternalToolDownloadState.Downloading));
            using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, linkedSource.Token);
            response.EnsureSuccessStatusCode();

            if (response.RequestMessage?.RequestUri is { } finalUri
                && !string.Equals(finalUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("externalTools.download.redirectInsecure");
            }

            var totalBytes = response.Content.Headers.ContentLength;
            if (totalBytes > options.MaxDownloadBytes)
            {
                throw new InvalidOperationException("externalTools.download.tooLarge");
            }

            long downloaded = 0;
            await using (var sourceStream = await response.Content.ReadAsStreamAsync(linkedSource.Token))
            await using (var targetStream = File.Create(targetPath))
            {
                var buffer = new byte[81920];
                while (true)
                {
                    var read = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length), linkedSource.Token);
                    if (read == 0)
                    {
                        break;
                    }

                    downloaded += read;
                    if (downloaded > options.MaxDownloadBytes)
                    {
                        throw new InvalidOperationException("externalTools.download.tooLarge");
                    }

                    await targetStream.WriteAsync(buffer.AsMemory(0, read), linkedSource.Token);
                    double? percentage = totalBytes is > 0 ? downloaded * 100d / totalBytes.Value : null;
                    progress?.Report(new ExternalToolDownloadProgress(totalBytes, downloaded, percentage, "externalTools.download.downloading", ExternalToolDownloadState.Downloading));
                }
            }

            progress?.Report(new ExternalToolDownloadProgress(totalBytes, downloaded, 100, "externalTools.download.validatingChecksum", ExternalToolDownloadState.ValidatingChecksum));
            var checksum = await checksumValidator.ValidateSha256Async(targetPath, source.Sha256, linkedSource.Token);
            if (!checksum.Success)
            {
                DeletePartial(targetPath);
                progress?.Report(new ExternalToolDownloadProgress(totalBytes, downloaded, 100, "externalTools.download.checksumInvalid", ExternalToolDownloadState.ChecksumInvalid));
                throw new InvalidOperationException("externalTools.download.checksumInvalid");
            }

            progress?.Report(new ExternalToolDownloadProgress(totalBytes, downloaded, 100, "externalTools.download.completed", ExternalToolDownloadState.ChecksumValid));
            return new DownloadedToolPackage(targetPath, source);
        }
        catch (OperationCanceledException)
        {
            DeletePartial(targetPath);
            progress?.Report(new ExternalToolDownloadProgress(null, 0, null, "externalTools.download.cancelled", ExternalToolDownloadState.Cancelled));
            throw;
        }
        catch
        {
            DeletePartial(targetPath);
            throw;
        }
    }

    private static void ValidateSource(ExternalToolPackageSource source)
    {
        if (!source.AutoDownloadEnabled)
        {
            throw new InvalidOperationException("externalTools.download.disabled");
        }

        if (!source.IsTrusted)
        {
            throw new InvalidOperationException("externalTools.download.untrustedSource");
        }

        if (string.IsNullOrWhiteSpace(source.Sha256))
        {
            throw new InvalidOperationException("externalTools.download.checksumRequired");
        }

        if (!Uri.TryCreate(source.DownloadUrl, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("externalTools.download.httpsRequired");
        }

        if (string.IsNullOrWhiteSpace(source.Version))
        {
            throw new InvalidOperationException("externalTools.download.versionRequired");
        }
    }

    private static void DeletePartial(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

public sealed class ExternalToolInstaller : IExternalToolInstaller
{
    private readonly IExternalToolDefinitionProvider definitionProvider;
    private readonly IExternalToolDownloader downloader;
    private readonly IChecksumValidator checksumValidator;
    private readonly IArchiveExtractor archiveExtractor;
    private readonly ExternalToolOptions options;
    private readonly ILogger<ExternalToolInstaller> logger;

    public ExternalToolInstaller(
        IExternalToolDefinitionProvider definitionProvider,
        IExternalToolDownloader downloader,
        IChecksumValidator checksumValidator,
        IArchiveExtractor archiveExtractor,
        ExternalToolOptions options,
        ILogger<ExternalToolInstaller> logger)
    {
        this.definitionProvider = definitionProvider;
        this.downloader = downloader;
        this.checksumValidator = checksumValidator;
        this.archiveExtractor = archiveExtractor;
        this.options = options;
        this.logger = logger;
    }

    public async Task<ExternalToolInstallResult> InstallAsync(
        ExternalToolInstallRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!request.UserConfirmed)
        {
            return ExternalToolInstallResult.Failed(request.ToolId, "externalTools.install.confirmationRequired");
        }

        var definition = definitionProvider.GetDefinitions()
            .SingleOrDefault(item => string.Equals(item.ToolId, request.ToolId, StringComparison.OrdinalIgnoreCase));
        if (definition is null)
        {
            return ExternalToolInstallResult.Failed(request.ToolId, "externalTools.tool.unknown");
        }

        if (definition.PackageSource is null)
        {
            return ExternalToolInstallResult.Failed(request.ToolId, "externalTools.install.downloadUnavailable");
        }

        try
        {
            var package = await downloader.DownloadAsync(definition.PackageSource, cancellationToken);
            var checksum = await checksumValidator.ValidateSha256Async(package.FilePath, definition.PackageSource.Sha256, cancellationToken);
            if (!checksum.Success)
            {
                return ExternalToolInstallResult.Failed(request.ToolId, "externalTools.install.checksumInvalid");
            }

            var finalDirectory = GetInstallDirectory(request.ToolId, definition.PackageSource.Version);
            var tempDirectory = Path.Combine(options.TemporaryDirectory, $"{request.ToolId}-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDirectory);

            var extraction = await archiveExtractor.ExtractAsync(package.FilePath, tempDirectory, cancellationToken);
            if (!extraction.Success)
            {
                return ExternalToolInstallResult.Failed(request.ToolId, extraction.ErrorMessageKey ?? "externalTools.install.extractFailed");
            }

            var executablePath = FindExecutable(tempDirectory, definition.ExecutableNames);
            if (executablePath is null)
            {
                return ExternalToolInstallResult.Failed(request.ToolId, "externalTools.install.executableMissing");
            }

            if (Directory.Exists(finalDirectory))
            {
                Directory.Delete(finalDirectory, recursive: true);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(finalDirectory) ?? options.ToolsRootDirectory);
            Directory.Move(tempDirectory, finalDirectory);
            var finalExecutable = Path.Combine(finalDirectory, Path.GetRelativePath(tempDirectory, executablePath));

            logger.LogInformation("Installed external tool {ToolId} to {InstallDirectory}", request.ToolId, finalDirectory);

            return new ExternalToolInstallResult(
                request.ToolId,
                true,
                ExternalToolState.Installed,
                finalExecutable,
                definition.PackageSource.Version,
                null,
                []);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or InvalidOperationException or NotSupportedException)
        {
            logger.LogWarning(ex, "Failed to install external tool {ToolId}", request.ToolId);
            return ExternalToolInstallResult.Failed(request.ToolId, "externalTools.install.failed");
        }
    }

    private string GetInstallDirectory(string toolId, string version)
    {
        return toolId switch
        {
            ExternalToolDefinitionProvider.NKitToolId => Path.Combine(options.ToolsRootDirectory, "nkit", $"v{version}"),
            ExternalToolDefinitionProvider.WitToolId or ExternalToolDefinitionProvider.WwtToolId => Path.Combine(options.ToolsRootDirectory, "wiimms"),
            _ => Path.Combine(options.ToolsRootDirectory, toolId)
        };
    }

    private static string? FindExecutable(string directory, IReadOnlyList<string> executableNames)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            if (executableNames.Any(name => string.Equals(Path.GetFileName(file), name, StringComparison.OrdinalIgnoreCase)))
            {
                return file;
            }
        }

        return null;
    }
}

public sealed class ExternalToolManager : IExternalToolManager
{
    private readonly IExternalToolDefinitionProvider definitionProvider;
    private readonly IExternalToolLocator locator;
    private readonly IExternalToolInstaller installer;
    private readonly IProcessRunner processRunner;
    private readonly ISettingsService settingsService;
    private readonly ISafePathService safePathService;
    private readonly ILogger<ExternalToolManager> logger;

    public ExternalToolManager(
        IExternalToolDefinitionProvider definitionProvider,
        IExternalToolLocator locator,
        IExternalToolInstaller installer,
        IProcessRunner processRunner,
        ISettingsService settingsService,
        ISafePathService safePathService,
        ILogger<ExternalToolManager> logger)
    {
        this.definitionProvider = definitionProvider;
        this.locator = locator;
        this.installer = installer;
        this.processRunner = processRunner;
        this.settingsService = settingsService;
        this.safePathService = safePathService;
        this.logger = logger;
    }

    public async Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default)
    {
        var statuses = new List<ExternalToolStatus>();
        foreach (var definition in definitionProvider.GetDefinitions())
        {
            statuses.Add(await CheckDefinitionAsync(definition, cancellationToken));
        }

        await PersistLastCheckAsync(statuses, cancellationToken);
        return statuses;
    }

    public async Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default)
    {
        var definition = GetDefinition(toolId);
        var status = await CheckDefinitionAsync(definition, cancellationToken);
        await PersistStatusAsync(status, cancellationToken);
        return status;
    }

    public async Task<ExternalToolInstallResult> InstallAsync(
        ExternalToolInstallRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await installer.InstallAsync(request, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.ExecutablePath))
        {
            return result;
        }

        var status = await ConfigureManualPathAsync(
            new ExternalToolManualPathRequest(request.ToolId, result.ExecutablePath),
            cancellationToken);

        return new ExternalToolInstallResult(
            request.ToolId,
            status.State is ExternalToolState.Installed or ExternalToolState.Configured,
            status.State,
            status.ExecutablePath,
            status.Version ?? result.Version,
            status.ErrorMessageKey,
            result.Warnings);
    }

    public async Task<ExternalToolStatus> ConfigureManualPathAsync(
        ExternalToolManualPathRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var definition = GetDefinition(request.ToolId);

        var path = safePathService.ValidateReadableFile(request.ExecutablePath);
        if (!path.Success || path.NormalizedPath is null)
        {
            return CreateStatus(definition, ExternalToolState.InvalidPath, null, null, "manual", path.ErrorKey ?? "externalTools.path.invalid");
        }

        if (!definition.ExecutableNames.Any(name => string.Equals(Path.GetFileName(path.NormalizedPath), name, StringComparison.OrdinalIgnoreCase)))
        {
            return CreateStatus(definition, ExternalToolState.InvalidPath, path.NormalizedPath, null, "manual", "externalTools.path.unexpectedExecutable");
        }

        var status = await ValidateLocatedToolAsync(definition, path.NormalizedPath, "manual", cancellationToken);
        if (status.State is ExternalToolState.Installed or ExternalToolState.Configured)
        {
            await PersistStatusAsync(status with { State = ExternalToolState.Configured }, cancellationToken);
            return status with { State = ExternalToolState.Configured };
        }

        return status;
    }

    private async Task<ExternalToolStatus> CheckDefinitionAsync(
        ExternalToolDefinition definition,
        CancellationToken cancellationToken)
    {
        var location = await locator.LocateAsync(definition, cancellationToken);
        if (!location.Found || location.ExecutablePath is null)
        {
            return CreateStatus(
                definition,
                definition.PackageSource is null ? ExternalToolState.NotInstalled : ExternalToolState.DownloadUnavailable,
                null,
                null,
                null,
                location.ErrorMessageKey,
                definition.PackageSource?.LocalPackagePath is not null ? "externalTools.action.installLocalOrManual" : "externalTools.action.configureManual");
        }

        return await ValidateLocatedToolAsync(definition, location.ExecutablePath, location.DetectionSource, cancellationToken);
    }

    private async Task<ExternalToolStatus> ValidateLocatedToolAsync(
        ExternalToolDefinition definition,
        string executablePath,
        string? detectionSource,
        CancellationToken cancellationToken)
    {
        if (definition.VersionArgument is null)
        {
            return CreateStatus(
                definition,
                ExternalToolState.Installed,
                executablePath,
                definition.PackageSource?.Version,
                detectionSource,
                null);
        }

        try
        {
            var result = await processRunner.RunAsync(
                new ProcessRequest(executablePath, [definition.VersionArgument], TimeSpan.FromSeconds(10)),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return CreateStatus(definition, ExternalToolState.InvalidVersion, executablePath, null, detectionSource, "externalTools.version.failed");
            }

            return CreateStatus(definition, ExternalToolState.Installed, executablePath, ParseVersion(result.StandardOutput), detectionSource, null);
        }
        catch (Exception ex) when (ex is IOException or TimeoutException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Failed to validate external tool {ToolId}", definition.ToolId);
            return CreateStatus(definition, ExternalToolState.InvalidVersion, executablePath, null, detectionSource, "externalTools.version.failed");
        }
    }

    private async Task PersistLastCheckAsync(IReadOnlyList<ExternalToolStatus> statuses, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync(cancellationToken);
        foreach (var status in statuses)
        {
            settings = ApplyStatus(settings, status);
        }

        await settingsService.SaveAsync(CopySettings(settings, externalToolsLastCheckAt: DateTimeOffset.UtcNow), cancellationToken);
    }

    private async Task PersistStatusAsync(ExternalToolStatus status, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync(cancellationToken);
        await settingsService.SaveAsync(CopySettings(ApplyStatus(settings, status), externalToolsLastCheckAt: DateTimeOffset.UtcNow), cancellationToken);
    }

    private static AppSettings ApplyStatus(AppSettings settings, ExternalToolStatus status)
    {
        if (status.State is not (ExternalToolState.Installed or ExternalToolState.Configured))
        {
            return settings;
        }

        return status.ToolId switch
        {
            ExternalToolDefinitionProvider.WitToolId => CopySettings(settings, witExecutablePath: status.ExecutablePath, witVersion: status.Version),
            ExternalToolDefinitionProvider.WwtToolId => CopySettings(settings, wwtExecutablePath: status.ExecutablePath, wwtVersion: status.Version),
            ExternalToolDefinitionProvider.NKitToolId => CopySettings(settings, nKitExecutablePath: status.ExecutablePath, nKitVersion: status.Version),
            _ => settings
        };
    }

    private static AppSettings CopySettings(
        AppSettings settings,
        string? witExecutablePath = null,
        string? wwtExecutablePath = null,
        string? nKitExecutablePath = null,
        string? witVersion = null,
        string? wwtVersion = null,
        string? nKitVersion = null,
        DateTimeOffset? externalToolsLastCheckAt = null)
    {
        return new AppSettings
        {
            Language = settings.Language,
            Theme = settings.Theme,
            AutoScanLibrary = settings.AutoScanLibrary,
            LibraryPath = settings.LibraryPath,
            ExportPath = settings.ExportPath,
            MaxParallelTasks = settings.MaxParallelTasks,
            EnableDiagnostics = settings.EnableDiagnostics,
            CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup,
            WitExecutablePath = witExecutablePath ?? settings.WitExecutablePath,
            WwtExecutablePath = wwtExecutablePath ?? settings.WwtExecutablePath,
            NKitExecutablePath = nKitExecutablePath ?? settings.NKitExecutablePath,
            WitVersion = witVersion ?? settings.WitVersion,
            WwtVersion = wwtVersion ?? settings.WwtVersion,
            NKitVersion = nKitVersion ?? settings.NKitVersion,
            ExternalToolsLastCheckAt = externalToolsLastCheckAt ?? settings.ExternalToolsLastCheckAt,
            CoverCachePath = settings.CoverCachePath,
            MetadataCachePath = settings.MetadataCachePath
        };
    }

    private ExternalToolDefinition GetDefinition(string toolId)
    {
        return definitionProvider.GetDefinitions()
            .SingleOrDefault(item => string.Equals(item.ToolId, toolId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"External tool '{toolId}' is not defined.");
    }

    private static ExternalToolStatus CreateStatus(
        ExternalToolDefinition definition,
        ExternalToolState state,
        string? executablePath,
        string? version,
        string? detectionSource,
        string? errorMessageKey,
        string? recommendedActionKey = null) =>
        new(
            definition.ToolId,
            definition.DisplayName,
            state,
            executablePath,
            version,
            detectionSource,
            errorMessageKey,
            DateTimeOffset.UtcNow,
            recommendedActionKey);

    private static string ParseVersion(string output)
    {
        var match = System.Text.RegularExpressions.Regex.Match(output, @"\d+(?:\.\d+)+(?:[a-zA-Z0-9.-]*)?");
        return match.Success ? match.Value : output.Trim();
    }
}
