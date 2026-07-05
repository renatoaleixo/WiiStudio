using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Wiimms;

public sealed class WwtServiceTests
{
    [Fact]
    public async Task AddGameAsync_ShouldRunWwtAddWithStructuredArguments()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WwtService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wwt"), NullLogger<WwtService>.Instance);

        await service.AddGameAsync(CreateDrive(), "/games/mario.iso", CancellationToken.None);

        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/opt/wiimms/wwt");
        processRunner.LastRequest.Arguments.Should().Equal(
            "add",
            "--part",
            "/media/wii",
            "--source",
            "/games/mario.iso",
            "--update",
            "--progress");
    }

    [Fact]
    public async Task RemoveGameAsync_ShouldRunWwtRemoveWithStructuredArguments()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WwtService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wwt"), NullLogger<WwtService>.Instance);

        await service.RemoveGameAsync(CreateDrive(), "RMCP01", CancellationToken.None);

        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/opt/wiimms/wwt");
        processRunner.LastRequest.Arguments.Should().Equal(
            "remove",
            "--part",
            "/media/wii",
            "RMCP01");
    }

    [Fact]
    public async Task AddGameAsync_ShouldThrowWhenWwtFails()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(4, string.Empty, "duplicate disc", TimeSpan.FromMilliseconds(10)));
        var service = new WwtService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wwt"), NullLogger<WwtService>.Instance);

        var act = () => service.AddGameAsync(CreateDrive(), "/games/mario.iso", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(4);
        exception.Which.StandardError.Should().Be("duplicate disc");
    }

    [Fact]
    public async Task RemoveGameAsync_ShouldThrowWhenWwtFails()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(5, string.Empty, "disc not found", TimeSpan.FromMilliseconds(10)));
        var service = new WwtService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wwt"), NullLogger<WwtService>.Instance);

        var act = () => service.RemoveGameAsync(CreateDrive(), "RMCP01", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(5);
        exception.Which.StandardError.Should().Be("disc not found");
    }

    private static UsbDrive CreateDrive()
    {
        return new UsbDrive("Wii HD", "/media/wii", UsbDriveFileSystem.Wbfs, 1000, 500);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        private readonly ProcessResult _result;

        public FakeProcessRunner(ProcessResult result)
        {
            _result = result;
        }

        public ProcessRequest? LastRequest { get; private set; }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;

            return Task.FromResult(_result);
        }
    }

    private sealed class FakeWiimmsToolLocator : IWiimmsToolLocator
    {
        private readonly string _wwtPath;

        public FakeWiimmsToolLocator(string wwtPath)
        {
            _wwtPath = wwtPath;
        }

        public Task<string> FindWitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult("/opt/wiimms/wit");
        }

        public Task<string> FindWwtAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_wwtPath);
        }
    }
}
