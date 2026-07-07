using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ICommandPaletteService
{
    Task<IReadOnlyList<CommandItem>> SearchAsync(string query, CancellationToken cancellationToken);

    Task ExecuteAsync(string commandId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CommandItem>> GetRecentCommandsAsync(CancellationToken cancellationToken);
}
