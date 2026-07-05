using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IUsbDetectionService
{
    Task<IReadOnlyList<UsbDriveDto>> ScanAsync(CancellationToken cancellationToken);

    IAsyncEnumerable<UsbDriveDto> WatchAsync(CancellationToken cancellationToken);
}
