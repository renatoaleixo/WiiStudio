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
    public async Task ListGamesAsync_ShouldRunWitListWithStructuredArguments()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(
            0,
            "RMCP01\tMario Kart Wii\tPAL\tWBFS\t1024\t/media/wii/wbfs/RMCP01.wbfs\n",
            string.Empty,
            TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, NullLogger<WitService>.Instance);

        await service.ListGamesAsync(CreateDrive(), CancellationToken.None);

        processRunner.LastRequest.Should().NotBeNull();
        processRunner.LastRequest!.ExecutablePath.Should().Be("wit");
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
        var service = new WitService(processRunner, NullLogger<WitService>.Instance);

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
        var service = new WitService(processRunner, NullLogger<WitService>.Instance);

        var games = await service.ListGamesAsync(CreateDrive(), CancellationToken.None);

        games.Should().ContainSingle().Which.GameId.Value.Should().Be("RSBP01");
    }

    [Fact]
    public async Task ListGamesAsync_ShouldThrowWhenWitFails()
    {
        var processRunner = new FakeProcessRunner(new ProcessResult(2, string.Empty, "device not found", TimeSpan.FromMilliseconds(10)));
        var service = new WitService(processRunner, NullLogger<WitService>.Instance);

        var act = () => service.ListGamesAsync(CreateDrive(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<WiimmsCommandException>();
        exception.Which.ExitCode.Should().Be(2);
        exception.Which.StandardError.Should().Be("device not found");
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
}
