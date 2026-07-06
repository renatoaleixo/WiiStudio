using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class LibraryViewService : ILibraryViewService
{
    private readonly IGameSearchService _gameSearchService;
    private readonly HashSet<string> _selectedGameIds = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<GameDto> _games = [];
    private LibraryViewMode _viewMode = LibraryViewMode.Detailed;

    public LibraryViewService(IGameSearchService gameSearchService)
    {
        _gameSearchService = gameSearchService;
    }

    public async Task<LibraryViewSnapshot> RefreshAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _gameSearchService.SearchAsync(request, cancellationToken);
        _games = result.Games;
        PruneSelection();

        var state = string.IsNullOrWhiteSpace(request.SearchText)
            ? LibraryViewState.Ready
            : LibraryViewState.Searching;

        return CreateSnapshot(result.TotalCount, state, result.SearchedAt);
    }

    public Task<LibraryViewSnapshot> ChangeViewModeAsync(LibraryViewMode mode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _viewMode = mode;

        return Task.FromResult(CreateSnapshot(_games.Count, LibraryViewState.Ready, DateTimeOffset.UtcNow));
    }

    public Task<LibraryViewSnapshot> SelectGamesAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        cancellationToken.ThrowIfCancellationRequested();

        _selectedGameIds.Clear();
        foreach (var gameId in gameIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            _selectedGameIds.Add(gameId);
        }

        PruneSelection();

        return Task.FromResult(CreateSnapshot(_games.Count, LibraryViewState.Selecting, DateTimeOffset.UtcNow));
    }

    private LibraryViewSnapshot CreateSnapshot(int totalCount, LibraryViewState state, DateTimeOffset refreshedAt)
    {
        return new LibraryViewSnapshot(
            _games,
            totalCount,
            _viewMode,
            _selectedGameIds.ToHashSet(StringComparer.OrdinalIgnoreCase),
            state,
            refreshedAt);
    }

    private void PruneSelection()
    {
        if (_selectedGameIds.Count == 0)
        {
            return;
        }

        var visibleGameIds = _games.Select(game => game.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _selectedGameIds.RemoveWhere(gameId => !visibleGameIds.Contains(gameId));
    }
}
