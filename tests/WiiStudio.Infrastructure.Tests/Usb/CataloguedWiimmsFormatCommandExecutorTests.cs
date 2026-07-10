using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Usb;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Usb;

public sealed class CataloguedWiimmsFormatCommandExecutorTests
{
    [Fact]
    public async Task ExecuteFormatWbfsAsync_ShouldRunCataloguedPrivilegedFormatCommand()
    {
        var privileged = new FakePrivilegedAccessService();
        var executor = new CataloguedWiimmsFormatCommandExecutor(
            new FakeWiimmsToolLocator("/usr/bin/wwt"),
            privileged,
            new FakeProcessRunner());
        var preview = CreatePreview();

        await executor.ExecuteFormatWbfsAsync(preview, null, CancellationToken.None);

        privileged.LastRequest.Should().NotBeNull();
        privileged.LastRequest!.ToolId.Should().Be("wwt");
        privileged.LastRequest.Operation.Should().Be(PrivilegedDeviceOperation.FormatWbfsPartition);
        privileged.LastRequest.DevicePath.Should().Be("/dev/sdc1");
        privileged.LastRequest.Arguments.Should().Equal("--force", "--verbose", "--progress", "FORMAT", "/dev/sdc1");
    }

    [Fact]
    public async Task ValidateWbfsAsync_ShouldRunCheckAndListCommands()
    {
        var processRunner = new FakeProcessRunner();
        var executor = new CataloguedWiimmsFormatCommandExecutor(
            new FakeWiimmsToolLocator("/usr/bin/wwt"),
            new FakePrivilegedAccessService(),
            processRunner);

        var valid = await executor.ValidateWbfsAsync(CreatePreview(), CancellationToken.None);

        valid.Should().BeTrue();
        processRunner.Requests.Should().HaveCount(2);
        processRunner.Requests[0].Arguments.Should().Equal("--part", "/dev/sdc1", "CHECK");
        processRunner.Requests[1].Arguments.Should().Equal("--part", "/dev/sdc1", "LIST", "--long", "--no-header");
    }

    private static UsbFormatPreview CreatePreview() =>
        new(
            "device-1",
            "partition-1",
            "Wii USB",
            "/dev/sdc",
            "/dev/sdc1",
            null,
            "FAT32",
            64L * 1024L * 1024L * 1024L,
            UsbPreparationMode.FormatAsWbfs,
            IsDestructive: true,
            IsBlocked: false,
            BlockReasonKey: null,
            UsbFormatPreparationService.WbfsConfirmationPhrase,
            null,
            []);

    private sealed class FakeWiimmsToolLocator : IWiimmsToolLocator
    {
        private readonly string wwtPath;

        public FakeWiimmsToolLocator(string wwtPath)
        {
            this.wwtPath = wwtPath;
        }

        public Task<string> FindWitAsync(CancellationToken cancellationToken) => Task.FromResult("/usr/bin/wit");

        public Task<string> FindWwtAsync(CancellationToken cancellationToken) => Task.FromResult(wwtPath);
    }

    private sealed class FakePrivilegedAccessService : IPrivilegedAccessService
    {
        public PrivilegedToolRequest? LastRequest { get; private set; }

        public Task<PrivilegedAccessStatus> CheckStatusAsync(PrivilegedAccessRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PrivilegedAccessStatus(OSPlatformKind.Linux, PrivilegedAccessState.Authorized, false, false, null, null, null));
        }

        public Task<PrivilegedProcessResult> RunKnownToolAsync(PrivilegedToolRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new PrivilegedProcessResult(
                0,
                "formatted",
                string.Empty,
                TimeSpan.FromMilliseconds(1),
                new PrivilegedAccessStatus(OSPlatformKind.Linux, PrivilegedAccessState.Authorized, false, false, null, null, null)));
        }
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public List<ProcessRequest> Requests { get; } = [];

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.FromMilliseconds(1)));
        }
    }
}
