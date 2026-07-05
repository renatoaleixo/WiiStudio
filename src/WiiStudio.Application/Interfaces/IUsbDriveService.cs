using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Interfaces;

public interface IUsbDriveService
{
    Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken);
}
