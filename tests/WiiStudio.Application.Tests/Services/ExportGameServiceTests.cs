using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class ExportGameServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldCreatePreviewForSelectedGames()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii", "/games/mario.wbfs");
        var service = CreateService([game], new FakeWitService(), new FakeExportFileSystem());

        var preview = await service.AnalyzeAsync(CreateRequest(["RMCP01"], "/exports", FileFormat.Iso), CancellationToken.None);

        var item = preview.Items.Should().ContainSingle().Subject;
        item.GameId.Should().Be("RMCP01");
        item.SourcePath.Should().Be("/games/mario.wbfs");
        item.DestinationPath.Should().Be("/exports/Mario Kart Wii [RMCP01].iso");
        item.WillExport.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldMarkConflictWhenDestinationExists()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii", "/games/mario.wbfs");
        var fileSystem = new FakeExportFileSystem(existingFiles: ["/exports/Mario Kart Wii [RMCP01].iso"]);
        var service = CreateService([game], new FakeWitService(), fileSystem);

        var preview = await service.AnalyzeAsync(
            CreateRequest(["RMCP01"], "/exports", FileFormat.Iso, ExportConflictResolution.Cancel),
            CancellationToken.None);

        var item = preview.Items.Should().ContainSingle().Subject;
        item.HasConflict.Should().BeTrue();
        item.WillExport.Should().BeFalse();
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRenameWhenConflictResolutionIsRename()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii", "/games/mario.wbfs");
        var fileSystem = new FakeExportFileSystem(existingFiles: ["/exports/Mario Kart Wii [RMCP01].wbfs"]);
        var service = CreateService([game], new FakeWitService(), fileSystem);

        var preview = await service.AnalyzeAsync(
            CreateRequest(["RMCP01"], "/exports", FileFormat.Wbfs, ExportConflictResolution.Rename),
            CancellationToken.None);

        preview.Items.Should().ContainSingle().Which.DestinationPath.Should().Be("/exports/Mario Kart Wii [RMCP01] (1).wbfs");
    }

    [Fact]
    public async Task ExportAsync_ShouldCopySelectedGames()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii", "/games/mario.wbfs");
        var witService = new FakeWitService();
        var fileSystem = new FakeExportFileSystem();
        var service = CreateService([game], witService, fileSystem);

        var result = await service.ExportAsync(CreateRequest(["RMCP01"], "/exports", FileFormat.Iso), CancellationToken.None);

        result.HasFailures.Should().BeFalse();
        result.Items.Should().ContainSingle().Which.Status.Should().Be(ExportStatus.Exported);
        witService.CopiedGames.Should().ContainSingle().Which.Should().Be(("/games/mario.wbfs", "/exports/Mario Kart Wii [RMCP01].iso", FileFormat.Iso));
        fileSystem.EnsuredDirectories.Should().ContainSingle().Which.Should().Be("/exports");
    }

    [Fact]
    public async Task ExportAsync_ShouldSkipConflictsWhenRequested()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii", "/games/mario.wbfs");
        var fileSystem = new FakeExportFileSystem(existingFiles: ["/exports/Mario Kart Wii [RMCP01].iso"]);
        var service = CreateService([game], new FakeWitService(), fileSystem);

        var result = await service.ExportAsync(
            CreateRequest(["RMCP01"], "/exports", FileFormat.Iso, ExportConflictResolution.Skip),
            CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Status.Should().Be(ExportStatus.Skipped);
    }

    [Fact]
    public async Task ExportAsync_ShouldThrowWhenCancelingConflicts()
    {
        var game = CreateGame("RMCP01", "Mario Kart Wii", "/games/mario.wbfs");
        var fileSystem = new FakeExportFileSystem(existingFiles: ["/exports/Mario Kart Wii [RMCP01].iso"]);
        var service = CreateService([game], new FakeWitService(), fileSystem);

        var act = () => service.ExportAsync(
            CreateRequest(["RMCP01"], "/exports", FileFormat.Iso, ExportConflictResolution.Cancel),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldThrowWhenGameIsMissing()
    {
        var service = CreateService([], new FakeWitService(), new FakeExportFileSystem());

        var act = () => service.AnalyzeAsync(CreateRequest(["RMCP01"], "/exports", FileFormat.Iso), CancellationToken.None);

        await act.Should().ThrowAsync<GameNotFoundException>();
    }

    private static ExportGameService CreateService(
        IReadOnlyList<GameDto> games,
        FakeWitService witService,
        FakeExportFileSystem fileSystem)
    {
        return new ExportGameService(new FakeGameLibraryService(games), witService, fileSystem);
    }

    private static ExportRequest CreateRequest(
        IReadOnlyList<string> gameIds,
        string destinationDirectory,
        FileFormat targetFormat,
        ExportConflictResolution conflictResolution = ExportConflictResolution.Overwrite)
    {
        return new ExportRequest(gameIds, destinationDirectory, targetFormat, conflictResolution);
    }

    private static GameDto CreateGame(string id, string title, string? filePath)
    {
        return new GameDto(id, title, Region.Pal, DiscType.Wii, FileFormat.Wbfs, filePath, 1024, DateTimeOffset.UtcNow);
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
        public List<(string SourcePath, string DestinationPath, FileFormat TargetFormat)> CopiedGames { get; } = [];

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

            return Task.CompletedTask;
        }

        public Task CopyAsync(string sourcePath, string destinationPath, FileFormat targetFormat, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CopiedGames.Add((sourcePath, destinationPath, targetFormat));

            return Task.CompletedTask;
        }
    }

    private sealed class FakeExportFileSystem : IExportFileSystem
    {
        private readonly HashSet<string> _existingFiles;

        public FakeExportFileSystem(IReadOnlyList<string>? existingFiles = null)
        {
            _existingFiles = new HashSet<string>(existingFiles ?? [], StringComparer.Ordinal);
        }

        public List<string> EnsuredDirectories { get; } = [];

        public bool FileExists(string path)
        {
            return _existingFiles.Contains(path);
        }

        public void EnsureDirectoryExists(string path)
        {
            EnsuredDirectories.Add(path);
        }
    }
}
