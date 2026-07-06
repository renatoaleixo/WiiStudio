using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDeviceManagerService
{
    Task<DeviceManagerSnapshot> RefreshAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken);

    Task<DeviceInfo> EjectAsync(string deviceId, CancellationToken cancellationToken);
}
