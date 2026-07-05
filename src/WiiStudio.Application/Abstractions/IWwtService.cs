using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Abstractions;

public interface IWwtService
{
    Task AddGameAsync(UsbDrive drive, string sourcePath, CancellationToken cancellationToken);

    Task RemoveGameAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken);
}
