using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IPrivilegedAccessService
{
    Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken = default);

    Task<PrivilegedProcessResult> RunKnownToolAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken = default);
}
