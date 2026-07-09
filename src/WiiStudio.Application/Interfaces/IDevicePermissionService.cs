using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDevicePermissionService
{
    Task<DevicePermissionStatus> CheckAsync(
        DevicePermissionRequest request,
        CancellationToken cancellationToken = default);

    Task<DevicePermissionStatus> RequestAuthorizationAsync(
        DevicePermissionRequest request,
        CancellationToken cancellationToken = default);
}
