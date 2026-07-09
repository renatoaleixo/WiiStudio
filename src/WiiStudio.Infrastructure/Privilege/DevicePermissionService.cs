using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class DevicePermissionService : IDevicePermissionService
{
    private readonly IPrivilegedAccessService _privilegedAccessService;

    public DevicePermissionService(IPrivilegedAccessService privilegedAccessService)
    {
        _privilegedAccessService = privilegedAccessService;
    }

    public async Task<DevicePermissionStatus> CheckAsync(
        DevicePermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var status = await _privilegedAccessService.CheckStatusAsync(
            new PrivilegedAccessRequest(request.ToolId, request.ExecutablePath, request.DevicePath, request.Operation),
            cancellationToken);
        return Map(status);
    }

    public Task<DevicePermissionStatus> RequestAuthorizationAsync(
        DevicePermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        return CheckAsync(request, cancellationToken);
    }

    private static DevicePermissionStatus Map(PrivilegedAccessStatus status)
    {
        return new DevicePermissionStatus(
            status.Platform,
            status.State,
            status.CanRetry,
            status.RequiresUserAction,
            status.Message,
            status.RecommendedAction,
            status.DiagnosticDetails);
    }
}
