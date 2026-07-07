namespace WiiStudio.Application.Interfaces;

public interface ICommandHistoryStore
{
    Task<IReadOnlyList<string>> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(IReadOnlyList<string> commandIds, CancellationToken cancellationToken);
}
