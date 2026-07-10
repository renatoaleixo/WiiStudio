using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Privilege;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Tests.Privilege;

public sealed class PolkitPolicyInstallerTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"wiistudio-polkit-installer-{Guid.NewGuid():N}");
    private readonly string originalPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

    [Fact]
    public async Task InstallAsync_ShouldUsePkexecInstallFallbackWhenPackagedInstallerIsMissing()
    {
        var bin = Path.Combine(root, "bin");
        Directory.CreateDirectory(bin);
        var pkexec = WriteExecutable(bin, "pkexec");
        var install = WriteExecutable(bin, "install");
        Environment.SetEnvironmentVariable("PATH", $"{bin}{Path.PathSeparator}{originalPath}");
        var expectedPolicy = Path.Combine(root, "system", "com.wiistudio.device.policy");
        var processRunner = new FakeProcessRunner();
        var installer = new PolkitPolicyInstaller(
            new FakeOperatingSystemInfo(OSPlatformKind.Linux),
            new FakePolkitPolicyLocator(expectedPolicy, exists: false),
            new FakePolkitPolicyValidator(expectedPolicy),
            processRunner);

        var result = await installer.InstallAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be(pkexec);
        processRunner.LastRequest.Arguments.Should().ContainInOrder(install, "-o", "root", "-g", "root", "-m", "0644");
        processRunner.LastRequest.Arguments.Last().Should().Be(expectedPolicy);
        File.Exists(processRunner.LastRequest.Arguments[^2]).Should().BeFalse("temporary policy file is cleaned after install");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("PATH", originalPath);
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string WriteExecutable(string directory, string name)
    {
        var path = Path.Combine(directory, name);
        File.WriteAllText(path, "#!/bin/sh\nexit 0\n");
        return path;
    }

    private sealed class FakeOperatingSystemInfo : IOperatingSystemInfo
    {
        public FakeOperatingSystemInfo(OSPlatformKind current)
        {
            Current = current;
        }

        public OSPlatformKind Current { get; }
    }

    private sealed class FakePolkitPolicyLocator : IPolkitPolicyLocator
    {
        private readonly bool exists;

        public FakePolkitPolicyLocator(string expectedPolicyPath, bool exists)
        {
            ExpectedPolicyPath = expectedPolicyPath;
            this.exists = exists;
        }

        public string ExpectedPolicyPath { get; }

        public Task<bool> ExistsAsync(CancellationToken cancellationToken = default) => Task.FromResult(exists);
    }

    private sealed class FakePolkitPolicyValidator : IPolkitPolicyValidator
    {
        private readonly string expectedPolicyPath;

        public FakePolkitPolicyValidator(string expectedPolicyPath)
        {
            this.expectedPolicyPath = expectedPolicyPath;
        }

        public Task<PolkitPolicyValidationResult> ValidateSourcePolicyAsync(string sourcePolicyPath, CancellationToken cancellationToken = default)
        {
            var content = File.ReadAllText(sourcePolicyPath);
            var valid = content.Contains("com.wiistudio.device.raw-access", StringComparison.Ordinal)
                && content.Contains("auth_admin_keep", StringComparison.Ordinal);
            return Task.FromResult(new PolkitPolicyValidationResult(valid, valid ? PolkitPolicyInstallState.Installed : PolkitPolicyInstallState.Invalid, sourcePolicyPath, valid, valid, valid, valid ? [] : ["invalid"]));
        }

        public Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(string installedPolicyPath, CancellationToken cancellationToken = default)
        {
            var valid = string.Equals(installedPolicyPath, expectedPolicyPath, StringComparison.Ordinal);
            return Task.FromResult(new PolkitPolicyValidationResult(valid, valid ? PolkitPolicyInstallState.Installed : PolkitPolicyInstallState.Invalid, installedPolicyPath, valid, valid, valid, valid ? [] : ["invalid"]));
        }
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public ProcessRequest? LastRequest { get; private set; }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.FromMilliseconds(1)));
        }
    }
}
