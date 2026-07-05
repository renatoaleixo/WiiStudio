using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.UseCases;

public sealed class DetectUsbDrivesUseCase
{
    private readonly IUsbDriveService _usbDriveService;

    public DetectUsbDrivesUseCase(IUsbDriveService usbDriveService)
    {
        _usbDriveService = usbDriveService;
    }

    public Task<IReadOnlyList<UsbDrive>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return _usbDriveService.DetectAsync(cancellationToken);
    }
}
