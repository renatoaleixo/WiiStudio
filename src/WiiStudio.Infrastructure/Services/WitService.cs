using WiiStudio.Application.Abstractions;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Infrastructure.Services;

public sealed class WitService : IWitService
{
    public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<IReadOnlyList<Game>>(Array.Empty<Game>());
    }
}
