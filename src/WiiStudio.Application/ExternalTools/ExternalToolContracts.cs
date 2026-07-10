namespace WiiStudio.Application.ExternalTools;

public interface IExternalToolManager
{
    Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(CancellationToken cancellationToken = default);

    Task<ExternalToolStatus> CheckAsync(string toolId, CancellationToken cancellationToken = default);

    Task<ExternalToolInstallResult> InstallAsync(
        ExternalToolInstallRequest request,
        CancellationToken cancellationToken = default);

    Task<ExternalToolStatus> ConfigureManualPathAsync(
        ExternalToolManualPathRequest request,
        CancellationToken cancellationToken = default);
}

public interface IExternalToolLocator
{
    Task<ExternalToolLocationResult> LocateAsync(
        ExternalToolDefinition definition,
        CancellationToken cancellationToken = default);
}

public interface IExternalToolInstaller
{
    Task<ExternalToolInstallResult> InstallAsync(
        ExternalToolInstallRequest request,
        CancellationToken cancellationToken = default);
}

public interface IExternalToolDownloader
{
    Task<DownloadedToolPackage> DownloadAsync(
        ExternalToolPackageSource source,
        CancellationToken cancellationToken = default);
}

public interface IExternalToolNetworkDownloader
{
    Task<DownloadedToolPackage> DownloadAsync(
        ExternalToolPackageSource source,
        IProgress<ExternalToolDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface IChecksumValidator
{
    Task<ChecksumValidationResult> ValidateSha256Async(
        string filePath,
        string expectedSha256,
        CancellationToken cancellationToken = default);
}

public interface IArchiveExtractor
{
    Task<ArchiveExtractionResult> ExtractAsync(
        string archivePath,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}

public enum ExternalToolState
{
    Unknown,
    NotInstalled,
    Installed,
    Configured,
    InvalidPath,
    InvalidVersion,
    DownloadAvailable,
    DownloadUnavailable,
    Installing,
    InstallFailed,
    UpdateAvailable,
    Offline
}

public enum ExternalToolDownloadState
{
    Pending,
    Confirming,
    Downloading,
    Downloaded,
    ValidatingChecksum,
    ChecksumValid,
    ChecksumInvalid,
    Cancelled,
    Failed,
    Offline
}

public sealed record ExternalToolDefinition(
    string ToolId,
    string DisplayName,
    IReadOnlyList<string> ExecutableNames,
    IReadOnlyList<string> SearchDirectories,
    string? VersionArgument,
    ExternalToolPackageSource? PackageSource,
    bool IsRequired);

public sealed record ExternalToolStatus(
    string ToolId,
    string DisplayName,
    ExternalToolState State,
    string? ExecutablePath,
    string? Version,
    string? DetectionSource,
    string? ErrorMessageKey,
    DateTimeOffset CheckedAt,
    string? RecommendedActionKey = null);

public sealed record ExternalToolPackageSource(
    string DownloadUrl,
    string Sha256,
    string Version,
    string SourceName,
    bool IsTrusted,
    bool AutoDownloadEnabled,
    string? LocalPackagePath = null);

public sealed record ExternalToolDownloadProgress(
    long? TotalBytes,
    long DownloadedBytes,
    double? Percentage,
    string? CurrentStatusKey,
    ExternalToolDownloadState State = ExternalToolDownloadState.Pending);

public sealed record ExternalToolLocationResult(
    bool Found,
    string? ExecutablePath,
    string? DetectionSource,
    string? ErrorMessageKey)
{
    public static ExternalToolLocationResult FoundAt(string executablePath, string source) =>
        new(true, executablePath, source, null);

    public static ExternalToolLocationResult Missing(string errorMessageKey = "externalTools.notInstalled") =>
        new(false, null, null, errorMessageKey);
}

public sealed record ExternalToolInstallRequest(string ToolId, bool UserConfirmed);

public sealed record ExternalToolManualPathRequest(string ToolId, string ExecutablePath);

public sealed record ExternalToolInstallResult(
    string ToolId,
    bool Success,
    ExternalToolState State,
    string? ExecutablePath,
    string? Version,
    string? ErrorMessageKey,
    IReadOnlyList<string> Warnings)
{
    public static ExternalToolInstallResult Failed(string toolId, string errorMessageKey, IReadOnlyList<string>? warnings = null) =>
        new(toolId, false, ExternalToolState.InstallFailed, null, null, errorMessageKey, warnings ?? []);
}

public sealed record DownloadedToolPackage(string FilePath, ExternalToolPackageSource Source);

public sealed record ChecksumValidationResult(bool Success, string ActualSha256, string ExpectedSha256);

public sealed record ArchiveExtractionResult(
    bool Success,
    string DestinationDirectory,
    IReadOnlyList<string> ExtractedFiles,
    string? ErrorMessageKey)
{
    public static ArchiveExtractionResult Failed(string destinationDirectory, string errorMessageKey) =>
        new(false, destinationDirectory, [], errorMessageKey);
}
