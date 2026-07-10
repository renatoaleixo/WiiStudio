using FluentAssertions;
using WiiStudio.Infrastructure.Privilege;

namespace WiiStudio.Infrastructure.Tests.Privilege;

public sealed class PolkitPolicyValidatorTests : IDisposable
{
    private readonly string _rootDirectory = Path.Combine(Path.GetTempPath(), $"wiistudio-polkit-{Guid.NewGuid():N}");

    [Fact]
    public async Task ValidateSourcePolicyAsync_ShouldAcceptOfficialPolicyShape()
    {
        var path = WritePolicy("""
            <?xml version="1.0" encoding="UTF-8"?>
            <policyconfig>
              <vendor>WiiStudio</vendor>
              <action id="com.wiistudio.device.raw-access">
                <description>Allow WiiStudio to access Wii USB/WBFS devices</description>
                <message>Authentication is required to access Wii USB/WBFS devices</message>
                <defaults>
                  <allow_any>auth_admin</allow_any>
                  <allow_inactive>auth_admin</allow_inactive>
                  <allow_active>auth_admin_keep</allow_active>
                </defaults>
              </action>
            </policyconfig>
            """);
        var validator = new PolkitPolicyValidator();

        var result = await validator.ValidateSourcePolicyAsync(path, CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.HasExpectedAction.Should().BeTrue();
        result.HasAuthAdminKeep.Should().BeTrue();
        result.HasExpectedVendor.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSourcePolicyAsync_ShouldRejectPolicyWithoutAuthAdminKeep()
    {
        var path = WritePolicy("""
            <policyconfig>
              <vendor>WiiStudio</vendor>
              <action id="com.wiistudio.device.raw-access">
                <defaults>
                  <allow_active>auth_admin</allow_active>
                </defaults>
              </action>
            </policyconfig>
            """);
        var validator = new PolkitPolicyValidator();

        var result = await validator.ValidateSourcePolicyAsync(path, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("auth_admin_keep", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, recursive: true);
        }
    }

    private string WritePolicy(string content)
    {
        Directory.CreateDirectory(_rootDirectory);
        var path = Path.Combine(_rootDirectory, "com.wiistudio.device.policy");
        File.WriteAllText(path, content);
        return path;
    }
}
