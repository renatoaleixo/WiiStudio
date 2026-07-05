using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Interfaces;

public interface ISettingsService
{
    Task<Settings> GetAsync(CancellationToken cancellationToken);

    Task SaveAsync(Settings settings, CancellationToken cancellationToken);
}
