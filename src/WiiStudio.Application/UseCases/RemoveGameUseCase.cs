using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.UseCases;

public sealed class RemoveGameUseCase
{
    private readonly IValidator<RemoveGameCommand> _validator;
    private readonly IUsbDriveService _usbDriveService;
    private readonly IWwtService _wwtService;
    private readonly IGameLibraryService _gameLibraryService;

    public RemoveGameUseCase(
        IValidator<RemoveGameCommand> validator,
        IUsbDriveService usbDriveService,
        IWwtService wwtService,
        IGameLibraryService gameLibraryService)
    {
        _validator = validator;
        _usbDriveService = usbDriveService;
        _wwtService = wwtService;
        _gameLibraryService = gameLibraryService;
    }

    public async Task ExecuteAsync(RemoveGameCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var drive = await FindDriveAsync(command.SourceDriveId, cancellationToken);
        await _wwtService.RemoveGameAsync(drive, command.GameId, cancellationToken);
        await _gameLibraryService.RefreshAsync(drive.Id, cancellationToken);
    }

    private async Task<Domain.Entities.UsbDrive> FindDriveAsync(Guid driveId, CancellationToken cancellationToken)
    {
        var drives = await _usbDriveService.DetectAsync(cancellationToken);

        return drives.FirstOrDefault(drive => drive.Id == driveId)
            ?? throw new UsbDriveNotFoundException(driveId);
    }
}
