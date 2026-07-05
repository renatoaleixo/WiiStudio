using WiiStudio.Domain.Entities;

namespace WiiStudio.Infrastructure.Usb;

public interface IUsbProvider
{
    Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken);
}
