using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Interfaces;

public interface IWwtService
{
    Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken);

    Task AddGameAsync(
        UsbDrive drive,
        string sourcePath,
        CancellationToken cancellationToken,
        IProgress<WiimmsOperationProgress>? progress = null,
        bool publishTask = true);

    Task RemoveGameAsync(UsbDrive drive, string gameId, CancellationToken cancellationToken);
}
