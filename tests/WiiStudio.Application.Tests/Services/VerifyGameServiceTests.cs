using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class VerifyGameServiceTests
{
    [Fact]
    public async Task VerifyAsync_ShouldReturnHealthyResultWhenWitSucceeds()
    {
        var game = CreateGame("RMCP01", "/games/mario.wbfs");
        var service = new VerifyGameService(new FakeGameLibraryService([game]), new FakeWitService());

        var report = await service.VerifyAsync(["RMCP01"], CancellationToken.None);

        report.HasProblems.Should().BeFalse();
        report.Results.Should().ContainSingle().Which.Status.Should().Be(VerificationStatus.Healthy);
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnCorruptedResultWhenWitFails()
    {
        var game = CreateGame("RMCP01", "/games/mario.wbfs");
        var service = new VerifyGameService(new FakeGameLibraryService([game]), new FakeWitService(new InvalidOperationException("bad dump")));

        var report = await service.VerifyAsync(["RMCP01"], CancellationToken.None);

        var result = report.Results.Should().ContainSingle().Subject;
        result.Status.Should().Be(VerificationStatus.Corrupted);
        result.Details.Should().Be("bad dump");
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnWarningWhenGameIsMissing()
    {
        var service = new VerifyGameService(new FakeGameLibraryService([]), new FakeWitService());

        var report = await service.VerifyAsync(["RMCP01"], CancellationToken.None);

        var result = report.Results.Should().ContainSingle().Subject;
        result.Status.Should().Be(VerificationStatus.Warning);
        result.Message.Should().Be("Game was not found in the library.");
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnUnsupportedWhenGameHasNoFilePath()
    {
        var game = CreateGame("RMCP01", null);
        var service = new VerifyGameService(new FakeGameLibraryService([game]), new FakeWitService());

        var report = await service.VerifyAsync(["RMCP01"], CancellationToken.None);

        report.Results.Should().ContainSingle().Which.Status.Should().Be(VerificationStatus.Unsupported);
    }

    [Fact]
    public async Task VerifyAsync_ShouldVerifyMultipleGames()
    {
        var service = new VerifyGameService(
            new FakeGameLibraryService([
                CreateGame("RMCP01", "/games/mario.wbfs"),
                CreateGame("RZDP01", "/games/zelda.wbfs")
            ]),
            new FakeWitService());

        var report = await service.VerifyAsync(["RMCP01", "RZDP01"], CancellationToken.None);

        report.TotalGames.Should().Be(2);
        report.Results.Should().OnlyContain(result => result.Status == VerificationStatus.Healthy);
    }

    private static GameDto CreateGame(string id, string? filePath)
    {
        return new GameDto(id, "Mario Kart Wii", Region.Pal, DiscType.Wii, FileFormat.Wbfs, filePath, 1024, DateTimeOffset.UtcNow);
    }

    private sealed class FakeGameLibraryService : IGameLibraryService
    {
        private readonly IReadOnlyList<GameDto> _games;

        public FakeGameLibraryService(IReadOnlyList<GameDto> games)
        {
            _games = games;
        }

        public Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_games);
        }

        public Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }

    private sealed class FakeWitService : IWitService
    {
        private readonly Exception? _verifyException;

        public FakeWitService(Exception? verifyException = null)
        {
            _verifyException = verifyException;
        }

        public Task<Version> GetVersionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Version(3, 5));
        }

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<Game>>([]);
        }

        public Task VerifyAsync(string sourcePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_verifyException is not null)
            {
                throw _verifyException;
            }

            return Task.CompletedTask;
        }

        public Task CopyAsync(string sourcePath, string destinationPath, FileFormat targetFormat, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
