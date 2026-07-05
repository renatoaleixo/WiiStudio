using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Abstractions;

public interface IWitService
{
    Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken);
}
