using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IUsbDeviceProvider
{
    Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default);
}
