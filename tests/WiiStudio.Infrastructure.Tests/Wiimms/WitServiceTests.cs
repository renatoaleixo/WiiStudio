using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Wiimms;

public sealed class WitServiceTests
{
    [Fact]
    public async Task GetVersionAsync_ShouldRunWitVersionWithResolvedExecutablePath()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(
            0,
            "wit: Wiimms ISO Tool v3.05a r8638\n",
            string.Empty,
            TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var version = await service.GetVersionAsync(CancellationToken.None);

        version.Should().Be(new Version(3, 5));
        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/opt/wiimms/wit");
        processRunner.LastRequest.Arguments.Should().Equal("--version");
    }

    [Fact]
    public async Task GetVersionAsync_ShouldThrowWhenWitVersionFails()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(127, string.Empty, "not found", TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var act = () => service.GetVersionAsync(CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(127);
        exception.Which.StandardError.Should().Be("not found");
    }

    [Fact]
    public async Task GetVersionAsync_ShouldThrowWhenVersionOutputCannotBeParsed()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(0, "wiimms iso tools", string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var act = () => service.GetVersionAsync(CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsVersionParseException>();
        exception.Which.StandardOutput.Should().Be("wiimms iso tools");
    }

    [Fact]
    public async Task ListGamesAsync_ShouldRunWitListWithStructuredArguments()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(
            0,
            "RMCP01\tMario Kart Wii\tPAL\tWBFS\t1024\t/media/wii/wbfs/RMCP01.wbfs\n",
            string.Empty,
            TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        await service.ListGamesAsync(CreateDrive(), CancellationToken.None);

        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/opt/wiimms/wit");
        processRunner.LastRequest.Arguments.Should().Equal(
            "list",
            "/media/wii",
            "--sections",
            "ID,TITLE,REGION,FORMAT,SIZE,FILE",
            "--no-header");
    }

    [Fact]
    public async Task ListGamesAsync_ShouldParseValidStructuredOutput()
    {
        const string output = """
            ID	TITLE	REGION	FORMAT	SIZE	FILE
            RMCP01	Mario Kart Wii	PAL	WBFS	1024	/media/wii/wbfs/RMCP01.wbfs
            RSBP01	Super Smash Bros. Brawl	NTSC-U	ISO	2048	/media/wii/wbfs/RSBP01.iso
        """;
        var processRunner = new FakeProcessRunner(new ProcessResult(0, output, string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var games = await service.ListGamesAsync(CreateDrive(), CancellationToken.None);

        games.Should().HaveCount(2);
        games[0].GameId.Value.Should().Be("RMCP01");
        games[0].Title.Should().Be("Mario Kart Wii");
        games[0].Region.Should().Be(Region.Pal);
        games[0].Format.Should().Be(FileFormat.Wbfs);
        games[0].FilePath.Should().Be("/media/wii/wbfs/RMCP01.wbfs");
        games[1].Region.Should().Be(Region.NtscU);
        games[1].Format.Should().Be(FileFormat.Iso);
    }

    [Fact]
    public async Task ListGamesAsync_ShouldIgnoreMalformedLines()
    {
        const string output = """
            invalid line
            BADID	Bad Game	PAL	WBFS	1024	/media/wii/bad.wbfs
            RMCP01	Mario Kart Wii	PAL	WBFS	not-a-number	/media/wii/wbfs/RMCP01.wbfs
            RSBP01	Super Smash Bros. Brawl	NTSC-U	ISO	2048	/media/wii/wbfs/RSBP01.iso
        """;
        var processRunner = new FakeProcessRunner(new ProcessResult(0, output, string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var games = await service.ListGamesAsync(CreateDrive(), CancellationToken.None);

        games.Should().ContainSingle().Which.GameId.Value.Should().Be("RSBP01");
    }

    [Fact]
    public async Task ListGamesAsync_ShouldThrowWhenWitFails()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(2, string.Empty, "device not found", TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var act = () => service.ListGamesAsync(CreateDrive(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(2);
        exception.Which.StandardError.Should().Be("device not found");
    }

    [Fact]
    public async Task VerifyAsync_ShouldRunWitVerifyWithStructuredArguments()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        await service.VerifyAsync("/games/mario.wbfs", CancellationToken.None);

        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/opt/wiimms/wit");
        processRunner.LastRequest.Arguments.Should().Equal(
            "verify",
            "/games/mario.wbfs",
            "--test");
    }

    [Fact]
    public async Task VerifyAsync_ShouldThrowWhenWitFails()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(4, string.Empty, "bad dump", TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var act = () => service.VerifyAsync("/games/mario.wbfs", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(4);
        exception.Which.StandardError.Should().Be("bad dump");
    }

    [Fact]
    public async Task CopyAsync_ShouldRunWitCopyWithStructuredArguments()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        await service.CopyAsync("/games/mario.wbfs", "/exports/Mario Kart Wii [RMCP01].iso", FileFormat.Iso, CancellationToken.None);

        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("/opt/wiimms/wit");
        processRunner.LastRequest.Arguments.Should().Equal(
            "copy",
            "--iso",
            "/games/mario.wbfs",
            "/exports/Mario Kart Wii [RMCP01].iso");
    }

    [Fact]
    public async Task CopyAsync_ShouldThrowWhenWitFails()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(6, string.Empty, "write failed", TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, new FakeWiimmsToolLocator("/opt/wiimms/wit"), NullLogger<WitService>.Instance);

        var act = () => service.CopyAsync("/games/mario.wbfs", "/exports/mario.iso", FileFormat.Iso, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(6);
        exception.Which.StandardError.Should().Be("write failed");
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
        private readonly string _witPath;

        public FakeWiimmsToolLocator(string witPath)
        {
            _witPath = witPath;
        }

        public Task<string> FindWitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_witPath);
        }

        public Task<string> FindWwtAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult("/opt/wiimms/wwt");
        }
    }
}
