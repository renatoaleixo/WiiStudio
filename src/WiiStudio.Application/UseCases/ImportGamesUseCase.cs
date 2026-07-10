using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.UseCases;

public sealed class ImportGamesUseCase
{
    private readonly IValidator<ImportGameCommand> _validator;
    private readonly IUsbDriveService _usbDriveService;
    private readonly IUsbWriteWorkflowService _usbWriteWorkflowService;
    private readonly IGameLibraryService _gameLibraryService;

    public ImportGamesUseCase(
        IValidator<ImportGameCommand> validator,
        IUsbDriveService usbDriveService,
        IUsbWriteWorkflowService usbWriteWorkflowService,
        IGameLibraryService gameLibraryService)
    {
        _validator = validator;
        _usbDriveService = usbDriveService;
        _usbWriteWorkflowService = usbWriteWorkflowService;
        _gameLibraryService = gameLibraryService;
    }

    public async Task ExecuteAsync(ImportGameCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var drive = await FindDriveAsync(command.DestinationDriveId, cancellationToken);
        foreach (var sourcePath in command.SourcePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var preview = await _usbWriteWorkflowService.CreatePreviewAsync(sourcePath, drive.Id, cancellationToken);
            if (preview.IsBlocked)
            {
                throw new InvalidOperationException(preview.BlockReasonKey ?? "usbWrite.blocked");
            }

            var result = await _usbWriteWorkflowService.ExecuteAsync(
                new UsbWriteExecutionRequest(preview, command.UserConfirmed),
                progress: null,
                cancellationToken);
            if (!result.Success)
            {
                throw new InvalidOperationException(result.ErrorMessageKey ?? "usbWrite.failed");
            }
        }

        await _gameLibraryService.RefreshAsync(drive.Id, cancellationToken);
    }

    private async Task<Domain.Entities.UsbDrive> FindDriveAsync(Guid driveId, CancellationToken cancellationToken)
    {
        var drives = await _usbDriveService.DetectAsync(cancellationToken);

        return drives.FirstOrDefault(drive => drive.Id == driveId)
            ?? throw new UsbDriveNotFoundException(driveId);
    }
}
