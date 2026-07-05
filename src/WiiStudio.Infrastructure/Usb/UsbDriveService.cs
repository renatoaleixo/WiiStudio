using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Infrastructure.Usb;

public sealed class UsbDriveService : IUsbDriveService
{
    private readonly IUsbProvider _usbProvider;

    public UsbDriveService(IUsbProvider usbProvider)
    {
        _usbProvider = usbProvider;
    }

    public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
    {
        return _usbProvider.DetectAsync(cancellationToken);
    }
}
