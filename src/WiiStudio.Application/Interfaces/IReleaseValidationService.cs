using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IReleaseValidationService
{
    IReadOnlyList<ReleaseValidationChecklistItem> GetChecklist();

    Task<ReleaseValidationPackage> CreateEvidencePackageAsync(
        ReleaseValidationRequest request,
        CancellationToken cancellationToken = default);
}
