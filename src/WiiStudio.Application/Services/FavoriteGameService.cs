using System.Collections.Concurrent;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class FavoriteGameService : IFavoriteGameService
{
    private readonly IGameLibraryService _gameLibraryService;
    private readonly ConcurrentDictionary<string, byte> _favoriteGameIds = new(StringComparer.OrdinalIgnoreCase);

    public FavoriteGameService(IGameLibraryService gameLibraryService)
    {
        _gameLibraryService = gameLibraryService;
    }

    public Task AddAsync(string gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateGameId(gameId);

        _favoriteGameIds.TryAdd(gameId.Trim().ToUpperInvariant(), 0);

        return Task.CompletedTask;
    }

    public async Task AddRangeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);

        foreach (var gameId in gameIds)
        {
            await AddAsync(gameId, cancellationToken);
        }
    }

    public Task RemoveAsync(string gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateGameId(gameId);

        _favoriteGameIds.TryRemove(gameId.Trim().ToUpperInvariant(), out _);

        return Task.CompletedTask;
    }

    public Task<bool> IsFavoriteAsync(string gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateGameId(gameId);

        return Task.FromResult(_favoriteGameIds.ContainsKey(gameId.Trim().ToUpperInvariant()));
    }

    public async Task<IReadOnlyList<GameDto>> GetFavoritesAsync(CancellationToken cancellationToken)
    {
        var games = await _gameLibraryService.GetGamesAsync(null, cancellationToken);
        var favorites = games
            .Where(game => _favoriteGameIds.ContainsKey(game.Id))
            .Select(game => game with { IsFavorite = true })
            .OrderBy(game => game.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        return favorites;
    }

    private static void ValidateGameId(string gameId)
    {
        if (!GameIdValidation.IsValid(gameId))
        {
            throw new ArgumentException("GameId must contain exactly 6 letters or digits.", nameof(gameId));
        }
    }
}
