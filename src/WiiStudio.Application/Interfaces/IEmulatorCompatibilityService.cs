using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IEmulatorCompatibilityService
{
    Task<EmulatorCompatibilityDto> GetAsync(string gameId, CancellationToken cancellationToken = default);
    Task<EmulatorCompatibilityDto> SaveAsync(EmulatorCompatibilityDto compatibility, CancellationToken cancellationToken = default);
}
