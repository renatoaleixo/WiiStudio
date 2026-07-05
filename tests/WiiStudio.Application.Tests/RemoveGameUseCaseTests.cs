using FluentAssertions;
using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Application.Validators;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests;

public sealed class RemoveGameUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRemoveGameFromSelectedDrive()
    {
        var drive = CreateDrive();
        var wwtService = new FakeWwtService();
        var useCase = new RemoveGameUseCase(
            new RemoveGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            wwtService);

        await useCase.ExecuteAsync(new RemoveGameCommand("RMCP01", drive.Id, Confirmed: true), CancellationToken.None);

        wwtService.RemovedGames.Should().ContainSingle().Which.Should().Be((drive, "RMCP01"));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenRemovalIsNotConfirmed()
    {
        var drive = CreateDrive();
        var useCase = new RemoveGameUseCase(
            new RemoveGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            new FakeWwtService());

        var act = () => useCase.ExecuteAsync(new RemoveGameCommand("RMCP01", drive.Id, Confirmed: false), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenDriveIsMissing()
    {
        var missingDriveId = Guid.NewGuid();
        var useCase = new RemoveGameUseCase(
            new RemoveGameCommandValidator(),
            new FakeUsbDriveService([]),
            new FakeWwtService());

        var act = () => useCase.ExecuteAsync(new RemoveGameCommand("RMCP01", missingDriveId, Confirmed: true), CancellationToken.None);

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
        public List<(UsbDrive Drive, string GameId)> RemovedGames { get; } = [];

        public Task AddGameAsync(UsbDrive drive, string sourcePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }

        public Task RemoveGameAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RemovedGames.Add((drive, gameId));

            return Task.CompletedTask;
        }
    }
}
