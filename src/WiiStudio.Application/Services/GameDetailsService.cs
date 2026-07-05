using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class GameDetailsService : IGameDetailsService
{
    private static readonly GameDetailsAction[] DefaultActions =
    [
        GameDetailsAction.Convert,
        GameDetailsAction.Export,
        GameDetailsAction.VerifyIntegrity,
        GameDetailsAction.Repair,
        GameDetailsAction.RefreshCovers,
        GameDetailsAction.ToggleFavorite,
        GameDetailsAction.Remove
    ];

    private readonly IGameLibraryService _gameLibraryService;
    private readonly IFavoriteGameService _favoriteGameService;

    public GameDetailsService(IGameLibraryService gameLibraryService, IFavoriteGameService favoriteGameService)
    {
        _gameLibraryService = gameLibraryService;
        _favoriteGameService = favoriteGameService;
    }

    public async Task<GameDetailsDto> GetAsync(string gameId, CancellationToken cancellationToken)
    {
        ValidateGameId(gameId);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedGameId = gameId.Trim().ToUpperInvariant();
        var games = await _gameLibraryService.GetGamesAsync(null, cancellationToken);
        var game = games.FirstOrDefault(candidate => string.Equals(candidate.Id, normalizedGameId, StringComparison.OrdinalIgnoreCase))
            ?? throw new GameNotFoundException(normalizedGameId);
        var isFavorite = await _favoriteGameService.IsFavoriteAsync(normalizedGameId, cancellationToken);

        return new GameDetailsDto(
            game.Id,
            game.Title,
            game.Region,
            game.DiscType,
            game.Format,
            game.FilePath,
            game.SizeInBytes,
            game.AddedAt,
            isFavorite,
            DefaultActions);
    }

    public async Task RefreshAsync(string gameId, CancellationToken cancellationToken)
    {
        await GetAsync(gameId, cancellationToken);
    }

    private static void ValidateGameId(string gameId)
    {
        if (!GameIdValidation.IsValid(gameId))
        {
            throw new ArgumentException("GameId must contain exactly 6 letters or digits.", nameof(gameId));
        }
    }
}
