using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly IKeyboardShortcutRegistry _registry;

    public KeyboardShortcutService(IKeyboardShortcutRegistry registry)
    {
        _registry = registry;
        RegisterDefaults();
    }

    public Task HandleAsync(string gesture, ShortcutContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _registry.TryGetByGesture(gesture, context.Scope, out _);
        return Task.CompletedTask;
    }

    public IReadOnlyList<KeyboardShortcut> GetActiveShortcuts(ShortcutScope scope)
    {
        return _registry.GetByScope(scope);
    }

    private void RegisterDefaults()
    {
        Register(new("command-palette", "shell.commandPalette.open", "Ctrl+Shift+P", "Shortcut.OpenCommandPalette", ShortcutScope.Global));
        Register(new("settings", "navigation.settings", "Ctrl+,", "Shortcut.OpenSettings", ShortcutScope.Global));
        Register(new("refresh", "shell.refresh", "Ctrl+R", "Shortcut.Refresh", ShortcutScope.Global));
        Register(new("search", "shell.search.focus", "Ctrl+F", "Shortcut.FocusSearch", ShortcutScope.Global));
        Register(new("help", "navigation.help", "F1", "Shortcut.OpenHelp", ShortcutScope.Global));
        Register(new("back", "navigation.back", "Alt+Left", "Shortcut.GoBack", ShortcutScope.Global));
        Register(new("forward", "navigation.forward", "Alt+Right", "Shortcut.GoForward", ShortcutScope.Global));
        Register(new("sidebar", "shell.sidebar.toggle", "Ctrl+B", "Shortcut.ToggleSidebar", ShortcutScope.Global));
    }

    private void Register(KeyboardShortcut shortcut)
    {
        if (!_registry.TryGetByGesture(shortcut.Gesture, shortcut.Scope, out _))
        {
            _registry.Register(shortcut);
        }
    }
}
