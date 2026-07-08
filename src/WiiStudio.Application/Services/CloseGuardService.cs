using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class CloseGuardService : ICloseGuardService
{
    public Task<CloseDecision> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CloseDecision.Allow);
    }
}
