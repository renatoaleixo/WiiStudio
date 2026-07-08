using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IKeyboardShortcutRegistry
{
    void Register(KeyboardShortcut shortcut);

    bool TryGetByGesture(string gesture, ShortcutScope scope, out KeyboardShortcut shortcut);

    IReadOnlyList<KeyboardShortcut> GetByScope(ShortcutScope scope);
}
