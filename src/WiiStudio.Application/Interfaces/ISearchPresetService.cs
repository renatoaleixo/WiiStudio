using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ISearchPresetService
{
    Task<SearchPreset> SaveAsync(string name, SearchRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<SearchPreset>> ListAsync(CancellationToken cancellationToken);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken);
}
