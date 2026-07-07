using WiiStudio.Application.Interfaces;

namespace WiiStudio.App.ViewModels;

public sealed class ShellCommandExecutor : ICommandExecutor
{
    private Func<string, CancellationToken, Task>? _handler;

    public void SetHandler(Func<string, CancellationToken, Task> handler)
    {
        _handler = handler;
    }

    public Task ExecuteAsync(string commandId, CancellationToken cancellationToken)
    {
        return _handler is null
            ? throw new InvalidOperationException("Shell command executor was not initialized.")
            : _handler(commandId, cancellationToken);
    }
}
