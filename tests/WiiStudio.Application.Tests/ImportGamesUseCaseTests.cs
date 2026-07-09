using FluentAssertions;
using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Application.Validators;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests;

public sealed class ImportGamesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldImportEverySourceIntoSelectedDrive()
    {
        var drive = CreateDrive();
        var wwtService = new FakeWwtService();
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            wwtService,
            new FakeGameLibraryService());

        await useCase.ExecuteAsync(new ImportGameCommand(["/games/mario.iso", "/games/zelda.wbfs"], drive.Id), CancellationToken.None);

        wwtService.AddedGames.Should().Equal(
            (drive, "/games/mario.iso"),
            (drive, "/games/zelda.wbfs"));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRefreshLibraryAfterImport()
    {
        var drive = CreateDrive();
        var gameLibraryService = new FakeGameLibraryService();
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            new FakeWwtService(),
            gameLibraryService);

        await useCase.ExecuteAsync(new ImportGameCommand(["/games/mario.iso"], drive.Id), CancellationToken.None);

        gameLibraryService.RefreshedDriveIds.Should().ContainSingle().Which.Should().Be(drive.Id);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenCommandIsInvalid()
    {
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([CreateDrive()]),
            new FakeWwtService(),
            new FakeGameLibraryService());

        var act = () => useCase.ExecuteAsync(new ImportGameCommand([], Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenDriveIsMissing()
    {
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([]),
            new FakeWwtService(),
            new FakeGameLibraryService());
        var missingDriveId = Guid.NewGuid();

        var act = () => useCase.ExecuteAsync(new ImportGameCommand(["/games/mario.iso"], missingDriveId), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<UsbDriveNotFoundException>();
        exception.Which.UsbDriveId.Should().Be(missingDriveId);
    }

    private static UsbDrive CreateDrive()
    {
        return new UsbDrive(Guid.NewGuid(), "/dev/sdb1", "/media/wii", "Wii HD", UsbDriveFileSystem.Wbfs, 1000, 500, 500, true, true);
    }

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_drives);
        }
    }

    private sealed class FakeWwtService : IWwtService
    {
        public List<(UsbDrive Drive, string SourcePath)> AddedGames { get; } = [];

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<Game>>([]);
        }

        public Task AddGameAsync(UsbDrive drive, string sourcePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AddedGames.Add((drive, sourcePath));

            return Task.CompletedTask;
        }

        public Task RemoveGameAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }

    private sealed class FakeGameLibraryService : IGameLibraryService
    {
        public List<Guid> RefreshedDriveIds { get; } = [];

        public Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<GameDto>>([]);
        }

        public Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RefreshedDriveIds.Add(usbDriveId);

            return Task.CompletedTask;
        }
    }
}
