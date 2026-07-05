using System.Collections.Concurrent;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class FavoriteGameService : IFavoriteGameService
{
    private readonly IGameLibraryService _gameLibraryService;
    private readonly IFavoriteGameStore _favoriteGameStore;
    private readonly ConcurrentDictionary<string, byte> _favoriteGameIds = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLoaded;

    public FavoriteGameService(IGameLibraryService gameLibraryService, IFavoriteGameStore favoriteGameStore)
    {
        _gameLibraryService = gameLibraryService;
        _favoriteGameStore = favoriteGameStore;
    }

    public async Task AddAsync(string gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateGameId(gameId);
        await EnsureLoadedAsync(cancellationToken);

        _favoriteGameIds.TryAdd(gameId.Trim().ToUpperInvariant(), 0);
        await SaveAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);

        foreach (var gameId in gameIds)
        {
            await AddAsync(gameId, cancellationToken);
        }
    }

    public async Task RemoveAsync(string gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateGameId(gameId);
        await EnsureLoadedAsync(cancellationToken);

        _favoriteGameIds.TryRemove(gameId.Trim().ToUpperInvariant(), out _);
        await SaveAsync(cancellationToken);
    }

    public async Task<bool> IsFavoriteAsync(string gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateGameId(gameId);
        await EnsureLoadedAsync(cancellationToken);

        return _favoriteGameIds.ContainsKey(gameId.Trim().ToUpperInvariant());
    }

    public async Task<IReadOnlyList<GameDto>> GetFavoritesAsync(CancellationToken cancellationToken)
    {
        await EnsureLoadedAsync(cancellationToken);

        var games = await _gameLibraryService.GetGamesAsync(null, cancellationToken);
        var favorites = games
            .Where(game => _favoriteGameIds.ContainsKey(game.Id))
            .Select(game => game with { IsFavorite = true })
            .OrderBy(game => game.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        return favorites;
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (_isLoaded)
        {
            return;
        }

        var favoriteGameIds = await _favoriteGameStore.LoadAsync(cancellationToken);
        foreach (var gameId in favoriteGameIds)
        {
            if (GameIdValidation.IsValid(gameId))
            {
                _favoriteGameIds.TryAdd(gameId.Trim().ToUpperInvariant(), 0);
            }
        }

        _isLoaded = true;
    }

    private Task SaveAsync(CancellationToken cancellationToken)
    {
        var gameIds = _favoriteGameIds.Keys
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _favoriteGameStore.SaveAsync(gameIds, cancellationToken);
    }

    private static void ValidateGameId(string gameId)
    {
        if (!GameIdValidation.IsValid(gameId))
        {
            throw new ArgumentException("GameId must contain exactly 6 letters or digits.", nameof(gameId));
        }
    }
}
