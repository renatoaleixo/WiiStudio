using System.Collections.Concurrent;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class SearchPresetService : ISearchPresetService
{
    private readonly ConcurrentDictionary<Guid, SearchPreset> _presets = new();

    public Task<SearchPreset> SaveAsync(string name, SearchRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var preset = new SearchPreset(Guid.NewGuid(), name.Trim(), request, DateTimeOffset.UtcNow);
        _presets[preset.Id] = preset;

        return Task.FromResult(preset);
    }

    public Task<IReadOnlyList<SearchPreset>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var presets = _presets.Values
            .OrderBy(preset => preset.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(preset => preset.CreatedAt)
            .ToArray();

        return Task.FromResult<IReadOnlyList<SearchPreset>>(presets);
    }

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _presets.TryRemove(id, out _);

        return Task.CompletedTask;
    }
}
