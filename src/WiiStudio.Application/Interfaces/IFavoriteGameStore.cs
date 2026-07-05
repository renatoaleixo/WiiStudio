namespace WiiStudio.Application.Interfaces;

public interface IFavoriteGameStore
{
    Task<IReadOnlySet<string>> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(IReadOnlySet<string> gameIds, CancellationToken cancellationToken);
}
