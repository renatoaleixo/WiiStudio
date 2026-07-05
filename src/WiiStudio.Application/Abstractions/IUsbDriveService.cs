using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Abstractions;

public interface IUsbDriveService
{
    Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken);
}
