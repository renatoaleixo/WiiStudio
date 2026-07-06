using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IProfileService
{
    Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken);

    Task<UserProfile> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken);

    Task<UserProfile> DuplicateAsync(Guid profileId, string name, CancellationToken cancellationToken);

    Task<UserProfile> RenameAsync(Guid profileId, string name, CancellationToken cancellationToken);

    Task SetActiveAsync(Guid profileId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid profileId, CancellationToken cancellationToken);

    Task ExportAsync(Guid profileId, string filePath, CancellationToken cancellationToken);

    Task<UserProfile> ImportAsync(string filePath, CancellationToken cancellationToken);
}
