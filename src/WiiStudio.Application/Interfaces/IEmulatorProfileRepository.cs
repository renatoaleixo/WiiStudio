using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IEmulatorProfileRepository
{
    Task<IReadOnlyList<EmulatorProfileDto>> GetProfilesAsync(CancellationToken cancellationToken = default);

    Task<EmulatorProfileDto?> GetByIdAsync(string profileId, CancellationToken cancellationToken = default);

    Task UpsertAsync(EmulatorProfileDto profile, CancellationToken cancellationToken = default);

    Task DeleteAsync(string profileId, CancellationToken cancellationToken = default);

    Task<GameEmulatorProfileAssignmentDto?> GetAssignmentAsync(
        string gameId,
        string emulatorId,
        CancellationToken cancellationToken = default);

    Task UpsertAssignmentAsync(GameEmulatorProfileAssignmentDto assignment, CancellationToken cancellationToken = default);
}
