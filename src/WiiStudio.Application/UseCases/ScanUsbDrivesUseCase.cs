using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.UseCases;

public sealed class ScanUsbDrivesUseCase
{
    private readonly IUsbDetectionService _usbDetectionService;

    public ScanUsbDrivesUseCase(IUsbDetectionService usbDetectionService)
    {
        _usbDetectionService = usbDetectionService;
    }

    public Task<IReadOnlyList<UsbDriveDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return _usbDetectionService.ScanAsync(cancellationToken);
    }
}
