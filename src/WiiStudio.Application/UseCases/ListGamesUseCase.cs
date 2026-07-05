using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Queries;

namespace WiiStudio.Application.UseCases;

public sealed class ListGamesUseCase
{
    private readonly IGameSearchService _gameSearchService;

    public ListGamesUseCase(IGameSearchService gameSearchService)
    {
        _gameSearchService = gameSearchService;
    }

    public async Task<SearchResult> ExecuteAsync(GetGamesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var request = new SearchRequest(
            query.UsbDriveId,
            query.SearchText,
            query.Regions,
            query.Formats,
            query.MinimumSizeInBytes,
            query.MaximumSizeInBytes,
            query.SortField,
            query.SortDescending);

        return await _gameSearchService.SearchAsync(request, cancellationToken);
    }
}
