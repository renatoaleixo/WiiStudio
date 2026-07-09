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
    public async Task ListGamesAsync_ShouldRunWwtListAndParseRawWbfsOutput()
    {
        const string output = """
            ID6     12/500 discs (27 GiB)
            ------------------------------------------------------------------------
            SFWE69  2010 FIFA World Cup South Africa
            SEME4Q  Disney Epic Mickey
            R43E69  EA Sports Active: Personal Trainer
            SJOE41  Just Dance 2014
            SJNE41  Just Dance 2016
            RMCP01  Mario Kart Wii
            RMKE01  Mario Sports Mix
            RNEEDA  Naruto Shippuden: Clash of Ninja Revolution 3
            PPNE01  New Super Mario Bros. Wii 2: The Next Levels
            STKE08  Tatsunoko vs. Capcom: Ultimate All-Stars
            RSPE01  Wii Sports
            RZTE01  Wii Sports Resort
            ------------------------------------------------------------------------
            Total: 12/500 discs, 28028 MiB ~ 27 GiB used, 210440 MiB ~ 206 GiB free.
            """;
        var processRunner = new FakeProcessRunner(new ProcessResult(0, output, string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WwtService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wwt"), NullLogger<WwtService>.Instance);

        var games = await service.ListGamesAsync(CreateRawDrive(), CancellationToken.None);

        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/opt/wiimms/wwt");
        processRunner.LastRequest.Arguments.Should().Equal("list", "--part", "/dev/sdc1");
        games.Should().HaveCount(12);
        games.Should().Contain(game => game.GameId.Value == "RMCP01" && game.Title == "Mario Kart Wii");
        games.Should().Contain(game => game.GameId.Value == "RZTE01" && game.Title == "Wii Sports Resort");
        games.All(game => game.Format == FileFormat.Wbfs).Should().BeTrue();
    }

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

    [Fact]
    public async Task ListGamesAsync_ShouldWrapPkexecTimeoutAsWiimmsCommandException()
    {
        var processRunner = new TimeoutAfterPermissionDeniedProcessRunner();
        var service = new WwtService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wwt"), NullLogger<WwtService>.Instance);

        var act = () => service.ListGamesAsync(CreateRawDrive(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(124);
        exception.Which.StandardError.Should().Contain("exceeded timeout");
    }

    private static UsbDrive CreateDrive()
    {
        return new UsbDrive("Wii HD", "/media/wii", UsbDriveFileSystem.Wbfs, 1000, 500);
    }

    private static UsbDrive CreateRawDrive()
    {
        return new UsbDrive(Guid.NewGuid(), "/dev/sdc1", "/dev/sdc1", "WBFS /dev/sdc1", UsbDriveFileSystem.Wbfs, 1000, 500, 500, true, true);
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

    private sealed class TimeoutAfterPermissionDeniedProcessRunner : IProcessRunner
    {
        private int callCount;

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            callCount++;
            if (callCount == 1)
            {
                return Task.FromResult(new ProcessResult(
                    48,
                    string.Empty,
                    "Can't open file: /dev/sdc1\n-> Permission denied [13]",
                    TimeSpan.FromMilliseconds(10)));
            }

            throw new TimeoutException($"Process '{request.ExecutablePath}' exceeded timeout {request.Timeout}.");
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
