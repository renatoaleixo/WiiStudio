using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ILibraryViewService
{
    Task<LibraryViewSnapshot> RefreshAsync(SearchRequest request, CancellationToken cancellationToken);

    Task<LibraryViewSnapshot> ChangeViewModeAsync(LibraryViewMode mode, CancellationToken cancellationToken);

    Task<LibraryViewSnapshot> SelectGamesAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);
}
