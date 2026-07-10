using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class PolkitPolicyLocator : IPolkitPolicyLocator
{
    public string ExpectedPolicyPath => PolkitPolicyConstants.ExpectedPolicyPath;

    public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ExpectedPolicyPath));
    }
}
