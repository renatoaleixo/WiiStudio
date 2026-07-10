using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Privilege;

namespace WiiStudio.Infrastructure.Tests.Privilege;

public sealed class PrivilegedAccessServiceTests : IDisposable
{
    private readonly string _rootDirectory = Path.Combine(Path.GetTempPath(), $"wiistudio-privilege-{Guid.NewGuid():N}");

    [Fact]
    public async Task RunKnownToolAsync_ShouldBlockUnknownTool()
    {
        var executablePath = CreateTool("bash");
        var strategy = new FakePrivilegeStrategy(OSPlatformKind.Linux);
        var service = CreateService(OSPlatformKind.Linux, strategy);

        var result = await service.RunKnownToolAsync(
            new PrivilegedToolRequest(
                "bash",
                executablePath,
                ["-c", "echo nope"],
                TimeSpan.FromSeconds(5),
                PrivilegedDeviceOperation.InspectDevice),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.AccessStatus.State.Should().Be(PrivilegedAccessState.Failed);
        strategy.RunCount.Should().Be(0);
    }

    [Fact]
    public async Task RunKnownToolAsync_ShouldBlockUnsafeArguments()
    {
        var executablePath = CreateTool("wwt");
        var strategy = new FakePrivilegeStrategy(OSPlatformKind.Linux);
        var service = CreateService(OSPlatformKind.Linux, strategy);

        var result = await service.RunKnownToolAsync(
            new PrivilegedToolRequest(
                "wwt",
                executablePath,
                ["list", "--part", "/dev/sdc1", "&&", "rm"],
                TimeSpan.FromSeconds(5),
                PrivilegedDeviceOperation.ListWbfsGames,
                DevicePath: "/dev/sdc1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.AccessStatus.State.Should().Be(PrivilegedAccessState.Failed);
        strategy.RunCount.Should().Be(0);
    }

    [Fact]
    public async Task RunKnownToolAsync_ShouldDispatchValidatedKnownTool()
    {
        var executablePath = CreateTool("wwt");
        var strategy = new FakePrivilegeStrategy(OSPlatformKind.Linux);
        var service = CreateService(OSPlatformKind.Linux, strategy);

        var result = await service.RunKnownToolAsync(
            new PrivilegedToolRequest(
                "wwt",
                executablePath,
                ["list", "--part", "/dev/sdc1"],
                TimeSpan.FromSeconds(5),
                PrivilegedDeviceOperation.ListWbfsGames,
                DevicePath: "/dev/sdc1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StandardOutput.Should().Be("ok");
        strategy.RunCount.Should().Be(1);
        strategy.LastRequest.Should().NotBeNull();
        strategy.LastRequest!.Operation.Should().Be(PrivilegedDeviceOperation.ListWbfsGames);
    }

    [Fact]
    public async Task RunKnownToolAsync_ShouldAllowCataloguedWbfsFormatCommand()
    {
        var executablePath = CreateTool("wwt");
        var strategy = new FakePrivilegeStrategy(OSPlatformKind.Linux);
        var service = CreateService(OSPlatformKind.Linux, strategy);

        var result = await service.RunKnownToolAsync(
            new PrivilegedToolRequest(
                "wwt",
                executablePath,
                ["--force", "--verbose", "--progress", "FORMAT", "/dev/sdc1"],
                TimeSpan.FromSeconds(5),
                PrivilegedDeviceOperation.FormatWbfsPartition,
                DevicePath: "/dev/sdc1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        strategy.RunCount.Should().Be(1);
    }

    [Fact]
    public async Task RunKnownToolAsync_ShouldRejectUncataloguedWbfsFormatArguments()
    {
        var executablePath = CreateTool("wwt");
        var strategy = new FakePrivilegeStrategy(OSPlatformKind.Linux);
        var service = CreateService(OSPlatformKind.Linux, strategy);

        var result = await service.RunKnownToolAsync(
            new PrivilegedToolRequest(
                "wwt",
                executablePath,
                ["--force", "--all", "FORMAT", "/dev/sdc1"],
                TimeSpan.FromSeconds(5),
                PrivilegedDeviceOperation.FormatWbfsPartition,
                DevicePath: "/dev/sdc1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        strategy.RunCount.Should().Be(0);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, recursive: true);
        }
    }

    private string CreateTool(string fileName)
    {
        Directory.CreateDirectory(_rootDirectory);
        var path = Path.Combine(_rootDirectory, fileName);
        File.WriteAllText(path, string.Empty);
        return path;
    }

    private static PrivilegedAccessService CreateService(
        OSPlatformKind platform,
        params IOperatingSystemPrivilegeStrategy[] strategies)
    {
        return new PrivilegedAccessService(
            new FakeOperatingSystemInfo(platform),
            [.. strategies, new FakePrivilegeStrategy(OSPlatformKind.Unknown)],
            NullLogger<PrivilegedAccessService>.Instance);
    }

    private sealed class FakeOperatingSystemInfo : IOperatingSystemInfo
    {
        public FakeOperatingSystemInfo(OSPlatformKind current)
        {
            Current = current;
        }

        public OSPlatformKind Current { get; }
    }

    private sealed class FakePrivilegeStrategy : IOperatingSystemPrivilegeStrategy
    {
        public FakePrivilegeStrategy(OSPlatformKind platform)
        {
            Platform = platform;
        }

        public OSPlatformKind Platform { get; }

        public int RunCount { get; private set; }

        public PrivilegedToolRequest? LastRequest { get; private set; }

        public Task<PrivilegedAccessStatus> CheckStatusAsync(
            PrivilegedAccessRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new PrivilegedAccessStatus(
                Platform,
                PrivilegedAccessState.AuthorizationRequired,
                CanRetry: true,
                RequiresUserAction: true,
                "ok",
                "ok",
                "ok"));
        }

        public Task<PrivilegedProcessResult> RunAsync(
            PrivilegedToolRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RunCount++;
            LastRequest = request;
            return Task.FromResult(new PrivilegedProcessResult(
                0,
                "ok",
                string.Empty,
                TimeSpan.FromMilliseconds(1),
                new PrivilegedAccessStatus(
                    Platform,
                    PrivilegedAccessState.Authorized,
                    CanRetry: false,
                    RequiresUserAction: false,
                    "ok",
                    "ok",
                    "ok")));
        }
    }
}
