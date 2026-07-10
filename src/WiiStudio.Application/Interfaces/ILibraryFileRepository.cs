using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ILibraryFileRepository
{
    Task UpsertAsync(GameDto game, CancellationToken cancellationToken);
}
