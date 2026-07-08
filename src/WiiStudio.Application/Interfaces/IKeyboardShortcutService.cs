using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IKeyboardShortcutService
{
    Task HandleAsync(string gesture, ShortcutContext context, CancellationToken cancellationToken = default);

    IReadOnlyList<KeyboardShortcut> GetActiveShortcuts(ShortcutScope scope);
}
