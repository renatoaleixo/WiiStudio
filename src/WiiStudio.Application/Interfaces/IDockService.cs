namespace WiiStudio.Application.Interfaces;

public interface IDockService
{
    Task SaveLayoutAsync(string layoutName, CancellationToken cancellationToken);

    Task LoadLayoutAsync(string layoutName, CancellationToken cancellationToken);

    Task ResetLayoutAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetLayoutsAsync(CancellationToken cancellationToken);
}
