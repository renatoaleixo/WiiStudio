using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameLibraryService
{
    Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken);

    Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken);
}
