using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IPlatformUsbDeviceProvider
{
    OSPlatformKind Platform { get; }

    Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default);
}
