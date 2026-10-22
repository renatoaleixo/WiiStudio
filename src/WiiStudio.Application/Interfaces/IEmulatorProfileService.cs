using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IEmulatorProfileService
{
    Task<IReadOnlyList<EmulatorProfileDto>> GetProfilesAsync(CancellationToken cancellationToken = default);

    Task<EmulatorProfileDto> SaveAsync(EmulatorProfileDto profile, CancellationToken cancellationToken = default);

    Task DeleteAsync(string profileId, CancellationToken cancellationToken = default);

    Task AssignToGameAsync(string gameId, string profileId, CancellationToken cancellationToken = default);

    Task<EmulatorProfileDto> ResolveForGameAsync(
        string gameId,
        string emulatorId = "dolphin",
        CancellationToken cancellationToken = default);

    Task ExportAsync(string profileId, string filePath, CancellationToken cancellationToken = default);

    Task<EmulatorProfileDto> ImportAsync(string filePath, CancellationToken cancellationToken = default);

    Task OpenEmulatorConfigurationAsync(CancellationToken cancellationToken = default);
}
