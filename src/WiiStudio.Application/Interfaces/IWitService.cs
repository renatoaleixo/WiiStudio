using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Interfaces;

public interface IWitService
{
    Task<Version> GetVersionAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken);
}
