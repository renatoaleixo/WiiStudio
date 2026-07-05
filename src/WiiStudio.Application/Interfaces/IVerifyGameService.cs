using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IVerifyGameService
{
    Task<VerificationReport> VerifyAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);
}
