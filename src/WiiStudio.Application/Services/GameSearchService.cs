using System.Globalization;
using System.Text;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class GameSearchService : IGameSearchService
{
    private readonly IGameLibraryService _gameLibraryService;
    private readonly IFavoriteGameService _favoriteGameService;

    public GameSearchService(IGameLibraryService gameLibraryService, IFavoriteGameService favoriteGameService)
    {
        _gameLibraryService = gameLibraryService;
        _favoriteGameService = favoriteGameService;
    }

    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var games = await _gameLibraryService.GetGamesAsync(request.UsbDriveId, cancellationToken);
        games = await MarkFavoritesAsync(games, cancellationToken);
        var query = games.AsEnumerable();

        query = ApplySearchText(query, request.SearchText);
        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        var resultGames = query.ToArray();

        return new SearchResult(resultGames, resultGames.Length, DateTimeOffset.UtcNow);
    }

    private async Task<IReadOnlyList<GameDto>> MarkFavoritesAsync(IReadOnlyList<GameDto> games, CancellationToken cancellationToken)
    {
        var result = new GameDto[games.Count];
        for (var index = 0; index < games.Count; index++)
        {
            var game = games[index];
            var isFavorite = await _favoriteGameService.IsFavoriteAsync(game.Id, cancellationToken);
            result[index] = game with { IsFavorite = isFavorite };
        }

        return result;
    }

    private static IEnumerable<GameDto> ApplySearchText(IEnumerable<GameDto> query, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        var normalizedSearchText = Normalize(searchText);

        return query.Where(game =>
            Normalize(game.Title).Contains(normalizedSearchText, StringComparison.Ordinal) ||
            Normalize(game.Id).Contains(normalizedSearchText, StringComparison.Ordinal) ||
            Normalize(game.Region.ToString()).Contains(normalizedSearchText, StringComparison.Ordinal));
    }

    private static IEnumerable<GameDto> ApplyFilters(IEnumerable<GameDto> query, SearchRequest request)
    {
        if (request.Regions is { Count: > 0 })
        {
            query = query.Where(game => request.Regions.Contains(game.Region));
        }

        if (request.Formats is { Count: > 0 })
        {
            query = query.Where(game => request.Formats.Contains(game.Format));
        }

        if (request.FavoritesOnly)
        {
            query = query.Where(game => game.IsFavorite);
        }

        if (request.MinimumSizeInBytes is not null)
        {
            query = query.Where(game => game.SizeInBytes >= request.MinimumSizeInBytes.Value);
        }

        if (request.MaximumSizeInBytes is not null)
        {
            query = query.Where(game => game.SizeInBytes <= request.MaximumSizeInBytes.Value);
        }

        return query;
    }

    private static IEnumerable<GameDto> ApplySorting(IEnumerable<GameDto> query, SearchRequest request)
    {
        return request.SortField switch
        {
            GameSortField.AddedAt => Sort(query, game => game.AddedAt, request.SortDescending),
            GameSortField.Size => Sort(query, game => game.SizeInBytes, request.SortDescending),
            GameSortField.Region => Sort(query, game => game.Region, request.SortDescending),
            GameSortField.GameId => Sort(query, game => game.Id, request.SortDescending, StringComparer.OrdinalIgnoreCase),
            GameSortField.Favorite => Sort(query, game => game.IsFavorite, !request.SortDescending)
                .ThenBy(game => game.Title, StringComparer.CurrentCultureIgnoreCase),
            _ => Sort(query, game => game.Title, request.SortDescending, StringComparer.CurrentCultureIgnoreCase)
        };
    }

    private static IOrderedEnumerable<GameDto> Sort<TKey>(
        IEnumerable<GameDto> query,
        Func<GameDto, TKey> keySelector,
        bool descending)
    {
        return descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    private static IOrderedEnumerable<GameDto> Sort(
        IEnumerable<GameDto> query,
        Func<GameDto, string> keySelector,
        bool descending,
        IComparer<string> comparer)
    {
        return descending
            ? query.OrderByDescending(keySelector, comparer)
            : query.OrderBy(keySelector, comparer);
    }

    private static string Normalize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
