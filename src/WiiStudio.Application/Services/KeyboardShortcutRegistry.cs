using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class KeyboardShortcutRegistry : IKeyboardShortcutRegistry
{
    private readonly Dictionary<(string Gesture, ShortcutScope Scope), KeyboardShortcut> _shortcuts = new();

    public void Register(KeyboardShortcut shortcut)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortcut.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(shortcut.CommandId);
        ArgumentException.ThrowIfNullOrWhiteSpace(shortcut.Gesture);

        var key = Normalize(shortcut.Gesture, shortcut.Scope);
        if (_shortcuts.ContainsKey(key))
        {
            throw new InvalidOperationException($"Shortcut '{shortcut.Gesture}' is already registered for scope '{shortcut.Scope}'.");
        }

        _shortcuts[key] = shortcut;
    }

    public bool TryGetByGesture(string gesture, ShortcutScope scope, out KeyboardShortcut shortcut)
    {
        return _shortcuts.TryGetValue(Normalize(gesture, scope), out shortcut!)
            || _shortcuts.TryGetValue(Normalize(gesture, ShortcutScope.Global), out shortcut!);
    }

    public IReadOnlyList<KeyboardShortcut> GetByScope(ShortcutScope scope)
    {
        return _shortcuts.Values
            .Where(shortcut => shortcut.Scope == scope)
            .OrderBy(shortcut => shortcut.Gesture, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static (string Gesture, ShortcutScope Scope) Normalize(string gesture, ShortcutScope scope)
    {
        return (gesture.Trim().ToUpperInvariant(), scope);
    }
}
