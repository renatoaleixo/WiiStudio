using WiiStudio.Application.DTOs;

namespace WiiStudio.Infrastructure.Privilege;

public interface IOperatingSystemPrivilegeStrategy
{
    OSPlatformKind Platform { get; }

    Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken);

    Task<PrivilegedProcessResult> RunAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken);
}
