using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Abstractions;

public interface ISettingsService
{
    Task<Settings> GetAsync(CancellationToken cancellationToken);

    Task SaveAsync(Settings settings, CancellationToken cancellationToken);
}
