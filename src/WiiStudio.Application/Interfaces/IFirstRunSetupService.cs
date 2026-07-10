using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IFirstRunSetupService
{
    Task<FirstRunSetupStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    Task<FirstRunSetupResult> CompleteAsync(
        FirstRunSetupRequest request,
        CancellationToken cancellationToken = default);
}
