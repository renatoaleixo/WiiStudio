namespace WiiStudio.Application.Interfaces;

public interface IPolkitPolicyLocator
{
    string ExpectedPolicyPath { get; }

    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
}
