using FluentAssertions;
using WiiStudio.Infrastructure.Security;

namespace WiiStudio.Infrastructure.Tests.Security;

public sealed class SecurityServicesTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wiistudio-security-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void SafePath_ShouldBlockPathTraversalOutsideRoot()
    {
        Directory.CreateDirectory(root);
        var service = new SafePathService();

        var result = service.EnsureInsideRoot(root, "../outside.txt");

        result.Success.Should().BeFalse();
        result.ErrorKey.Should().Be("security.path.traversal");
    }

    [Fact]
    public void SafePath_ShouldValidateCompoundGameExtensions()
    {
        Directory.CreateDirectory(root);
        var gamePath = Path.Combine(root, "game.nkit.iso");
        File.WriteAllText(gamePath, string.Empty);
        var service = new SafePathService();

        var result = service.ValidateExtension(gamePath, [".iso", ".wbfs", ".rvz", ".nkit.iso"]);

        result.Success.Should().BeTrue();
        result.NormalizedPath.Should().EndWith("game.nkit.iso");
    }

    [Fact]
    public void FileSystemPermissions_ShouldDenyPluginAccessOutsidePluginStorage()
    {
        var service = new FileSystemPermissionService(new SafePathService());

        var result = service.Evaluate(new FileSystemPermissionRequest(
            Path.Combine(root, "settings.json"),
            FileSystemAccess.Read,
            FileSystemPermissionScope.AppData,
            PluginId: "com.example.plugin"));

        result.Allowed.Should().BeFalse();
        result.ReasonKey.Should().Be("security.filesystem.pluginScopeDenied");
    }

    [Fact]
    public void LogRedaction_ShouldRedactTokensAuthorizationAndUrlsWithCredentials()
    {
        var service = new LogRedactionService();
        const string input = "token=abc123 Authorization: Bearer xyz https://user:pass@example.com";

        var redacted = service.Redact(input);

        redacted.Should().Contain("token=[REDACTED]");
        redacted.Should().Contain("Authorization: Bearer [REDACTED]");
        redacted.Should().NotContain("abc123");
        redacted.Should().NotContain("user:pass");
    }

    [Fact]
    public void SensitiveSettings_ShouldInferSecretsAndBlockExport()
    {
        var service = new SensitiveSettingsService();

        service.GetSensitivity("metadata.apiToken").Should().Be(SettingSensitivity.Secret);
        service.CanExport("metadata.apiToken").Should().BeFalse();
        service.ProtectForDisplay("metadata.apiToken", "abcdef123456").Should().Be("[REDACTED]");
    }

    [Fact]
    public void BackupSecurity_ShouldRejectUnsafeEntryPaths()
    {
        var service = new BackupSecurityService(new SafePathService());

        var result = service.ValidateEntry("../settings.json", "../settings.json", new string('a', 64));

        result.Success.Should().BeFalse();
        result.ErrorKey.Should().Be("security.backup.entryPathInvalid");
    }

    [Fact]
    public void UpdateSecurity_ShouldRequireHttpsAndBlockDowngrade()
    {
        var service = new UpdateSecurityService();

        var insecure = service.Validate(new UpdateSecurityMetadata(
            "1.1.0",
            "http://example.com/update.zip",
            new string('a', 64),
            "Changes",
            IsCritical: false), "1.0.0");

        var downgrade = service.Validate(new UpdateSecurityMetadata(
            "0.9.0",
            "https://example.com/update.zip",
            new string('a', 64),
            "Changes",
            IsCritical: false), "1.0.0");

        insecure.ErrorKey.Should().Be("security.update.httpsRequired");
        downgrade.ErrorKey.Should().Be("security.update.downgradeBlocked");
    }

    [Fact]
    public void SecurityChecklist_ShouldBlockMissingSecurityEvidence()
    {
        var service = new SecurityChecklistService();

        var result = service.Evaluate(new SecurityChecklistInput(
            PathsNormalized: true,
            PathTraversalBlocked: true,
            UsesExternalProcess: true,
            ExternalProcessUsesRunner: false,
            HandlesSecrets: true,
            LogsRedacted: false,
            PluginFeature: true,
            PluginPermissionsValidated: true,
            BackupFeature: true,
            BackupIntegrityValidated: false,
            SecurityTestsAdded: false));

        result.Passed.Should().BeFalse();
        result.Blockers.Should().Contain([
            "security.checklist.processRunnerRequired",
            "security.checklist.redactionRequired",
            "security.checklist.backupIntegrityRequired",
            "security.checklist.testsRequired"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
