using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Interfaces;

public interface ISettingsService
{
    Task<Settings> GetAsync(CancellationToken cancellationToken);

    Task<Settings> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(Settings settings, CancellationToken cancellationToken);

    Task ResetAsync(CancellationToken cancellationToken);

    Task ExportAsync(string filePath, CancellationToken cancellationToken);

    Task ImportAsync(string filePath, CancellationToken cancellationToken);
}
