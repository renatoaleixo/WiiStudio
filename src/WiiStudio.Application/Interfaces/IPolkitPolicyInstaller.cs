using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IPolkitPolicyInstaller
{
    Task<PolkitPolicyInstallStatus> CheckAsync(CancellationToken cancellationToken = default);

    Task<PolkitPolicyInstallResult> InstallAsync(CancellationToken cancellationToken = default);

    Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(CancellationToken cancellationToken = default);
}
