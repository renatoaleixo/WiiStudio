using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IGameSearchService
{
    Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}
