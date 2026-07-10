using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IPolkitPolicyValidator
{
    Task<PolkitPolicyValidationResult> ValidateSourcePolicyAsync(
        string sourcePolicyPath,
        CancellationToken cancellationToken = default);

    Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(
        string installedPolicyPath,
        CancellationToken cancellationToken = default);
}
