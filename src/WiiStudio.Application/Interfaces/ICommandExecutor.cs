namespace WiiStudio.Application.Interfaces;

public interface ICommandExecutor
{
    Task ExecuteAsync(string commandId, CancellationToken cancellationToken);
}
