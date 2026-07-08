using System.Security.Cryptography;
using System.Text.RegularExpressions;
using WiiStudio.Plugin.Abstractions;
using WiiStudio.Plugin.Security;

namespace WiiStudio.Infrastructure.Security;

public interface ISafePathService
{
    SafePathResult Normalize(string path);
    SafePathResult EnsureInsideRoot(string rootPath, string candidatePath);
    SafePathResult ValidateExtension(string path, IReadOnlyList<string> allowedExtensions);
    SafePathResult ValidateWritableDestination(string path, bool allowOverwrite = false);
    SafePathResult ValidateReadableFile(string path);
}

public interface IFileSystemPermissionService
{
    FileSystemPermissionResult Evaluate(FileSystemPermissionRequest request);
}

public interface ILogRedactionService
{
    string Redact(string input);
    IReadOnlyDictionary<string, string> Redact(IReadOnlyDictionary<string, string> values);
    bool ContainsSensitiveData(string input);
    string HashForCorrelation(string input);
}

public interface ISensitiveSettingsService
{
    SettingDefinition Define(string key, SettingSensitivity sensitivity, bool exportable, bool requiresRestart = false);
    SettingSensitivity GetSensitivity(string key);
    bool CanExport(string key);
    string ProtectForDisplay(string key, string? value);
}

public interface IBackupSecurityService
{
    BackupSecurityResult ValidateEntry(string entryPath, string relativePath, string sha256);
    BackupSecurityResult ValidateDestination(string outputFile);
    string ComputeSha256(Stream stream);
}

public interface IUpdateSecurityService
{
    UpdateSecurityResult Validate(UpdateSecurityMetadata metadata, string currentVersion);
    UpdateSecurityResult ValidatePackage(string packagePath, string expectedSha256);
}

public interface ISecurityChecklistService
{
    SecurityChecklistResult Evaluate(SecurityChecklistInput input);
}

public sealed class SafePathService : ISafePathService
{
    private const int MaxPathLength = 4096;

    public SafePathResult Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return SafePathResult.Fail("security.path.empty");
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return SafePathResult.Fail("security.path.invalidCharacters");
        }

        if (path.Length > MaxPathLength)
        {
            return SafePathResult.Fail("security.path.tooLong");
        }

        if (ContainsEncodedTraversal(path))
        {
            return SafePathResult.Fail("security.path.traversal");
        }

        try
        {
            var normalized = Path.GetFullPath(path);
            return SafePathResult.Ok(normalized);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return SafePathResult.Fail("security.path.invalid");
        }
    }

    public SafePathResult EnsureInsideRoot(string rootPath, string candidatePath)
    {
        var root = Normalize(rootPath);
        if (!root.Success || root.NormalizedPath is null)
        {
            return root;
        }

        var candidate = Normalize(Path.IsPathRooted(candidatePath)
            ? candidatePath
            : Path.Combine(root.NormalizedPath, candidatePath));

        if (!candidate.Success || candidate.NormalizedPath is null)
        {
            return candidate;
        }

        var normalizedRoot = EnsureTrailingSeparator(root.NormalizedPath);
        if (!candidate.NormalizedPath.StartsWith(normalizedRoot, PathComparison))
        {
            return SafePathResult.Fail("security.path.outsideRoot");
        }

        var warnings = IsSymbolicLink(candidate.NormalizedPath)
            ? new[] { "security.path.symlink" }
            : [];

        return SafePathResult.Ok(candidate.NormalizedPath, warnings);
    }

    public SafePathResult ValidateExtension(string path, IReadOnlyList<string> allowedExtensions)
    {
        ArgumentNullException.ThrowIfNull(allowedExtensions);

        var normalized = Normalize(path);
        if (!normalized.Success || normalized.NormalizedPath is null)
        {
            return normalized;
        }

        if (allowedExtensions.Count == 0)
        {
            return normalized;
        }

        var fileName = Path.GetFileName(normalized.NormalizedPath);
        var allowed = allowedExtensions.Any(extension =>
            fileName.EndsWith(NormalizeExtension(extension), StringComparison.OrdinalIgnoreCase));

        return allowed
            ? normalized
            : SafePathResult.Fail("security.path.extensionNotAllowed");
    }

    public SafePathResult ValidateWritableDestination(string path, bool allowOverwrite = false)
    {
        var normalized = Normalize(path);
        if (!normalized.Success || normalized.NormalizedPath is null)
        {
            return normalized;
        }

        var directory = Path.GetDirectoryName(normalized.NormalizedPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return SafePathResult.Fail("security.path.directoryMissing");
        }

        if (!allowOverwrite && File.Exists(normalized.NormalizedPath))
        {
            return SafePathResult.Fail("security.path.overwriteBlocked");
        }

        return normalized;
    }

    public SafePathResult ValidateReadableFile(string path)
    {
        var normalized = Normalize(path);
        if (!normalized.Success || normalized.NormalizedPath is null)
        {
            return normalized;
        }

        return File.Exists(normalized.NormalizedPath)
            ? normalized
            : SafePathResult.Fail("security.path.fileMissing");
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith('.') ? extension : "." + extension;

    private static bool ContainsEncodedTraversal(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("../", StringComparison.Ordinal)
            || normalized.EndsWith("/..", StringComparison.Ordinal)
            || normalized.Contains("%2e%2e", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSymbolicLink(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return false;
        }

        var attributes = File.GetAttributes(path);
        return attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}

public sealed class FileSystemPermissionService : IFileSystemPermissionService
{
    private readonly ISafePathService safePathService;

    public FileSystemPermissionService(ISafePathService safePathService)
    {
        this.safePathService = safePathService;
    }

    public FileSystemPermissionResult Evaluate(FileSystemPermissionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalized = request.RootPath is null
            ? safePathService.Normalize(request.Path)
            : safePathService.EnsureInsideRoot(request.RootPath, request.Path);

        if (!normalized.Success || normalized.NormalizedPath is null)
        {
            return FileSystemPermissionResult.Denied(normalized.ErrorKey ?? "security.path.invalid");
        }

        if (request.PluginId is not null && request.Scope is not FileSystemPermissionScope.PluginStorage and not FileSystemPermissionScope.UserSelectedFile and not FileSystemPermissionScope.UserSelectedDirectory)
        {
            return FileSystemPermissionResult.Denied("security.filesystem.pluginScopeDenied");
        }

        if (request.Access == FileSystemAccess.Delete && request.Scope is FileSystemPermissionScope.AppData or FileSystemPermissionScope.PluginStorage)
        {
            return FileSystemPermissionResult.Denied("security.filesystem.deleteDenied");
        }

        var warnings = new List<string>();
        if (request.Access is FileSystemAccess.Write or FileSystemAccess.Delete)
        {
            warnings.Add("security.filesystem.destructive");
        }

        if (request.Scope == FileSystemPermissionScope.ExternalDevice)
        {
            warnings.Add("security.filesystem.externalDevice");
        }

        return new FileSystemPermissionResult(true, normalized.NormalizedPath, null, warnings);
    }
}

public sealed class LogRedactionService : ILogRedactionService
{
    private const string Redacted = "[REDACTED]";

    private static readonly Regex[] SensitivePatterns =
    [
        new(@"(?<key>password|pwd|token|api_key|apikey|secret|connectionString|access_token|refresh_token|sig)=([^\s;,\}]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new(@"(?<key>authorization)\s*:\s*(?<scheme>bearer\s+)?[^\s;,\}]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new(@"bearer\s+[a-z0-9._\-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new(@"SharedAccessSignature\s+[^\s;,\}]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new(@"[a-z][a-z0-9+\-.]*://[^:\s/]+:[^@\s/]+@[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
    ];

    public string Redact(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var output = input;
        output = SensitivePatterns[0].Replace(output, match => $"{match.Groups["key"].Value}={Redacted}");
        output = SensitivePatterns[1].Replace(output, match =>
        {
            var scheme = match.Groups["scheme"].Success ? match.Groups["scheme"].Value : string.Empty;
            return $"{match.Groups["key"].Value}: {scheme}{Redacted}";
        });

        for (var index = 2; index < SensitivePatterns.Length; index++)
        {
            output = SensitivePatterns[index].Replace(output, Redacted);
        }

        return output;
    }

    public IReadOnlyDictionary<string, string> Redact(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return values.ToDictionary(
            item => item.Key,
            item => IsSensitiveKey(item.Key) ? Redacted : Redact(item.Value),
            StringComparer.OrdinalIgnoreCase);
    }

    public bool ContainsSensitiveData(string input) =>
        !string.IsNullOrEmpty(input) && SensitivePatterns.Any(pattern => pattern.IsMatch(input));

    public string HashForCorrelation(string input)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return $"sha256:{Convert.ToHexString(hash)[..12].ToLowerInvariant()}";
    }

    private static bool IsSensitiveKey(string key) =>
        key.Contains("password", StringComparison.OrdinalIgnoreCase)
        || key.Contains("token", StringComparison.OrdinalIgnoreCase)
        || key.Contains("secret", StringComparison.OrdinalIgnoreCase)
        || key.Contains("apiKey", StringComparison.OrdinalIgnoreCase)
        || key.Contains("authorization", StringComparison.OrdinalIgnoreCase)
        || key.Contains("connectionString", StringComparison.OrdinalIgnoreCase);
}

public sealed class SensitiveSettingsService : ISensitiveSettingsService
{
    private readonly Dictionary<string, SettingDefinition> definitions = new(StringComparer.OrdinalIgnoreCase);

    public SettingDefinition Define(string key, SettingSensitivity sensitivity, bool exportable, bool requiresRestart = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var definition = new SettingDefinition(key, sensitivity, exportable, requiresRestart);
        definitions[key] = definition;
        return definition;
    }

    public SettingSensitivity GetSensitivity(string key)
    {
        if (definitions.TryGetValue(key, out var definition))
        {
            return definition.Sensitivity;
        }

        return InferSensitivity(key);
    }

    public bool CanExport(string key)
    {
        if (definitions.TryGetValue(key, out var definition))
        {
            return definition.Exportable;
        }

        return GetSensitivity(key) is SettingSensitivity.Public or SettingSensitivity.Internal;
    }

    public string ProtectForDisplay(string key, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return GetSensitivity(key) switch
        {
            SettingSensitivity.Secret => "[REDACTED]",
            SettingSensitivity.Sensitive => MaskPartial(value),
            _ => value
        };
    }

    private static SettingSensitivity InferSensitivity(string key)
    {
        if (key.Contains("token", StringComparison.OrdinalIgnoreCase)
            || key.Contains("secret", StringComparison.OrdinalIgnoreCase)
            || key.Contains("password", StringComparison.OrdinalIgnoreCase)
            || key.Contains("apiKey", StringComparison.OrdinalIgnoreCase))
        {
            return SettingSensitivity.Secret;
        }

        if (key.Contains("path", StringComparison.OrdinalIgnoreCase)
            || key.Contains("url", StringComparison.OrdinalIgnoreCase))
        {
            return SettingSensitivity.Sensitive;
        }

        return SettingSensitivity.Internal;
    }

    private static string MaskPartial(string value)
    {
        if (value.Length <= 6)
        {
            return "***";
        }

        return $"{value[..3]}***{value[^3..]}";
    }
}

public sealed class BackupSecurityService : IBackupSecurityService
{
    private readonly ISafePathService safePathService;

    public BackupSecurityService(ISafePathService safePathService)
    {
        this.safePathService = safePathService;
    }

    public BackupSecurityResult ValidateEntry(string entryPath, string relativePath, string sha256)
    {
        if (string.IsNullOrWhiteSpace(entryPath) || Path.IsPathRooted(entryPath) || entryPath.Contains("..", StringComparison.Ordinal))
        {
            return BackupSecurityResult.Fail("security.backup.entryPathInvalid");
        }

        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) || relativePath.Contains("..", StringComparison.Ordinal))
        {
            return BackupSecurityResult.Fail("security.backup.relativePathInvalid");
        }

        if (!Regex.IsMatch(sha256, "^[a-fA-F0-9]{64}$"))
        {
            return BackupSecurityResult.Fail("security.backup.hashInvalid");
        }

        return BackupSecurityResult.Ok();
    }

    public BackupSecurityResult ValidateDestination(string outputFile)
    {
        var result = safePathService.ValidateExtension(outputFile, [".zip"]);
        if (!result.Success)
        {
            return BackupSecurityResult.Fail(result.ErrorKey ?? "security.backup.destinationInvalid");
        }

        var destination = safePathService.ValidateWritableDestination(outputFile, allowOverwrite: true);
        return destination.Success
            ? BackupSecurityResult.Ok()
            : BackupSecurityResult.Fail(destination.ErrorKey ?? "security.backup.destinationInvalid");
    }

    public string ComputeSha256(Stream stream)
    {
        var originalPosition = stream.CanSeek ? stream.Position : 0;
        var hash = SHA256.HashData(stream);
        if (stream.CanSeek)
        {
            stream.Position = originalPosition;
        }

        return Convert.ToHexString(hash);
    }
}

public sealed class UpdateSecurityService : IUpdateSecurityService
{
    public UpdateSecurityResult Validate(UpdateSecurityMetadata metadata, string currentVersion)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (!Uri.TryCreate(metadata.DownloadUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            return UpdateSecurityResult.Fail("security.update.httpsRequired");
        }

        if (!Version.TryParse(metadata.Version, out var updateVersion) || !Version.TryParse(currentVersion, out var installedVersion))
        {
            return UpdateSecurityResult.Fail("security.update.versionInvalid");
        }

        if (updateVersion < installedVersion)
        {
            return UpdateSecurityResult.Fail("security.update.downgradeBlocked");
        }

        if (!Regex.IsMatch(metadata.Sha256, "^[a-fA-F0-9]{64}$"))
        {
            return UpdateSecurityResult.Fail("security.update.hashInvalid");
        }

        return UpdateSecurityResult.Ok(metadata.IsCritical ? ["security.update.critical"] : []);
    }

    public UpdateSecurityResult ValidatePackage(string packagePath, string expectedSha256)
    {
        if (!File.Exists(packagePath))
        {
            return UpdateSecurityResult.Fail("security.update.packageMissing");
        }

        using var stream = File.OpenRead(packagePath);
        var actual = Convert.ToHexString(SHA256.HashData(stream));
        return string.Equals(actual, expectedSha256, StringComparison.OrdinalIgnoreCase)
            ? UpdateSecurityResult.Ok([])
            : UpdateSecurityResult.Fail("security.update.checksumMismatch");
    }
}

public sealed class SecurityChecklistService : ISecurityChecklistService
{
    public SecurityChecklistResult Evaluate(SecurityChecklistInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var blockers = new List<string>();
        AddBlockerIf(!input.PathsNormalized, "security.checklist.pathsNormalized", blockers);
        AddBlockerIf(!input.PathTraversalBlocked, "security.checklist.pathTraversalBlocked", blockers);
        AddBlockerIf(input.UsesExternalProcess && !input.ExternalProcessUsesRunner, "security.checklist.processRunnerRequired", blockers);
        AddBlockerIf(input.HandlesSecrets && !input.LogsRedacted, "security.checklist.redactionRequired", blockers);
        AddBlockerIf(input.PluginFeature && !input.PluginPermissionsValidated, "security.checklist.pluginPermissionsRequired", blockers);
        AddBlockerIf(input.BackupFeature && !input.BackupIntegrityValidated, "security.checklist.backupIntegrityRequired", blockers);
        AddBlockerIf(!input.SecurityTestsAdded, "security.checklist.testsRequired", blockers);

        return new SecurityChecklistResult(blockers.Count == 0, blockers);
    }

    private static void AddBlockerIf(bool condition, string blocker, ICollection<string> blockers)
    {
        if (condition)
        {
            blockers.Add(blocker);
        }
    }
}

public sealed class PluginSecurityPolicy
{
    private readonly IPluginPermissionPolicy permissionPolicy;

    public PluginSecurityPolicy(IPluginPermissionPolicy permissionPolicy)
    {
        this.permissionPolicy = permissionPolicy;
    }

    public PluginSecurityDecision Evaluate(string pluginId, IReadOnlyList<PluginPermission> permissions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        ArgumentNullException.ThrowIfNull(permissions);

        var warnings = new List<string>();
        foreach (var permission in permissions)
        {
            var decision = permissionPolicy.Evaluate(permission);
            if (decision == PermissionDecision.RequiresUserApproval)
            {
                warnings.Add($"security.plugin.permissionRequiresApproval:{permission}");
            }
        }

        return new PluginSecurityDecision(true, warnings);
    }
}

public enum FileSystemAccess
{
    Read,
    Write,
    Delete,
    Enumerate
}

public enum FileSystemPermissionScope
{
    UserSelectedFile,
    UserSelectedDirectory,
    AppData,
    Cache,
    PluginStorage,
    BackupDirectory,
    ExportDirectory,
    ExternalDevice
}

public enum SettingSensitivity
{
    Public,
    Internal,
    Sensitive,
    Secret
}

public sealed record SafePathResult(bool Success, string? NormalizedPath, string? ErrorKey, IReadOnlyList<string> Warnings)
{
    public static SafePathResult Ok(string normalizedPath, IReadOnlyList<string>? warnings = null) =>
        new(true, normalizedPath, null, warnings ?? []);

    public static SafePathResult Fail(string errorKey) =>
        new(false, null, errorKey, []);
}

public sealed record FileSystemPermissionRequest(
    string Path,
    FileSystemAccess Access,
    FileSystemPermissionScope Scope,
    string? RootPath = null,
    string? PluginId = null);

public sealed record FileSystemPermissionResult(
    bool Allowed,
    string? NormalizedPath,
    string? ReasonKey,
    IReadOnlyList<string> Warnings)
{
    public static FileSystemPermissionResult Denied(string reasonKey) => new(false, null, reasonKey, []);
}

public sealed record SettingDefinition(string Key, SettingSensitivity Sensitivity, bool Exportable, bool RequiresRestart);

public sealed record BackupSecurityResult(bool Success, string? ErrorKey)
{
    public static BackupSecurityResult Ok() => new(true, null);
    public static BackupSecurityResult Fail(string errorKey) => new(false, errorKey);
}

public sealed record UpdateSecurityMetadata(
    string Version,
    string DownloadUrl,
    string Sha256,
    string Changelog,
    bool IsCritical,
    string? SignatureUrl = null);

public sealed record UpdateSecurityResult(bool Success, string? ErrorKey, IReadOnlyList<string> Warnings)
{
    public static UpdateSecurityResult Ok(IReadOnlyList<string> warnings) => new(true, null, warnings);
    public static UpdateSecurityResult Fail(string errorKey) => new(false, errorKey, []);
}

public sealed record SecurityChecklistInput(
    bool PathsNormalized,
    bool PathTraversalBlocked,
    bool UsesExternalProcess,
    bool ExternalProcessUsesRunner,
    bool HandlesSecrets,
    bool LogsRedacted,
    bool PluginFeature,
    bool PluginPermissionsValidated,
    bool BackupFeature,
    bool BackupIntegrityValidated,
    bool SecurityTestsAdded);

public sealed record SecurityChecklistResult(bool Passed, IReadOnlyList<string> Blockers);

public sealed record PluginSecurityDecision(bool Allowed, IReadOnlyList<string> Warnings);
