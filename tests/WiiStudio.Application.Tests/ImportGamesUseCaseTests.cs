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
        var workflowService = new FakeUsbWriteWorkflowService();
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            workflowService,
            new FakeGameLibraryService());

        await useCase.ExecuteAsync(new ImportGameCommand(["/games/mario.iso", "/games/zelda.wbfs"], drive.Id, UserConfirmed: true), CancellationToken.None);

        workflowService.ExecutedSources.Should().Equal("/games/mario.iso", "/games/zelda.wbfs");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRefreshLibraryAfterImport()
    {
        var drive = CreateDrive();
        var gameLibraryService = new FakeGameLibraryService();
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            new FakeUsbWriteWorkflowService(),
            gameLibraryService);

        await useCase.ExecuteAsync(new ImportGameCommand(["/games/mario.iso"], drive.Id, UserConfirmed: true), CancellationToken.None);

        gameLibraryService.RefreshedDriveIds.Should().ContainSingle().Which.Should().Be(drive.Id);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRequireUserConfirmation()
    {
        var drive = CreateDrive();
        var workflowService = new FakeUsbWriteWorkflowService();
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            workflowService,
            new FakeGameLibraryService());

        var act = () => useCase.ExecuteAsync(new ImportGameCommand(["/games/mario.iso"], drive.Id, UserConfirmed: false), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("usbWrite.confirmationRequired");
        workflowService.ExecutedSources.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenCommandIsInvalid()
    {
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([CreateDrive()]),
            new FakeUsbWriteWorkflowService(),
            new FakeGameLibraryService());

        var act = () => useCase.ExecuteAsync(new ImportGameCommand([], Guid.NewGuid(), UserConfirmed: true), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowWhenDriveIsMissing()
    {
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([]),
            new FakeUsbWriteWorkflowService(),
            new FakeGameLibraryService());
        var missingDriveId = Guid.NewGuid();

        var act = () => useCase.ExecuteAsync(new ImportGameCommand(["/games/mario.iso"], missingDriveId, UserConfirmed: true), CancellationToken.None);

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

    private sealed class FakeUsbWriteWorkflowService : IUsbWriteWorkflowService
    {
        public List<string> ExecutedSources { get; } = [];

        public Task<UsbWritePreview> CreatePreviewAsync(string sourcePath, Guid targetDriveId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new UsbWritePreview(
                sourcePath,
                GameId: null,
                targetDriveId,
                "Wii HD",
                "/dev/sdb1",
                "/media/wii",
                UsbDriveFileSystem.Wbfs,
                SourceSizeBytes: 100,
                RequiredBytes: 100,
                FreeBytes: 500,
                RequiresFat32Split: false,
                UserConfirmationRequired: true,
                IsBlocked: false,
                BlockReasonKey: null,
                PermissionStatus: null,
                Warnings: []));
        }

        public Task<UsbWriteResult> ExecuteAsync(UsbWriteExecutionRequest request, IProgress<WiimmsOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!request.UserConfirmed)
            {
                return Task.FromResult(new UsbWriteResult(false, request.Preview.GameId, request.Preview.TargetDriveId, "usbWrite.confirmationRequired", TimeSpan.Zero));
            }

            ExecutedSources.Add(request.Preview.SourcePath);
            return Task.FromResult(new UsbWriteResult(true, request.Preview.GameId, request.Preview.TargetDriveId, null, TimeSpan.FromMilliseconds(1)));
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
