using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.UseCases;

public sealed class ImportGamesUseCase
{
    private readonly IValidator<ImportGameCommand> _validator;
    private readonly IUsbDriveService _usbDriveService;
    private readonly IWwtService _wwtService;

    public ImportGamesUseCase(
        IValidator<ImportGameCommand> validator,
        IUsbDriveService usbDriveService,
        IWwtService wwtService)
    {
        _validator = validator;
        _usbDriveService = usbDriveService;
        _wwtService = wwtService;
    }

    public async Task ExecuteAsync(ImportGameCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var drive = await FindDriveAsync(command.DestinationDriveId, cancellationToken);
        foreach (var sourcePath in command.SourcePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _wwtService.AddGameAsync(drive, sourcePath, cancellationToken);
        }
    }

    private async Task<Domain.Entities.UsbDrive> FindDriveAsync(Guid driveId, CancellationToken cancellationToken)
    {
        var drives = await _usbDriveService.DetectAsync(cancellationToken);

        return drives.FirstOrDefault(drive => drive.Id == driveId)
            ?? throw new UsbDriveNotFoundException(driveId);
    }
}
