using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IEmulatorCompatibilityRepository
{
    Task<EmulatorCompatibilityDto?> GetAsync(string gameId, string emulatorId, CancellationToken cancellationToken = default);
    Task UpsertAsync(EmulatorCompatibilityDto compatibility, CancellationToken cancellationToken = default);
}
