using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ICloseGuardService
{
    Task<CloseDecision> CanCloseAsync(CancellationToken cancellationToken = default);
}
