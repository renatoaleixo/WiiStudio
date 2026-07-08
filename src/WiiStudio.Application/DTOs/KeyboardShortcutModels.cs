namespace WiiStudio.Application.DTOs;

public enum ShortcutScope
{
    Global,
    Shell,
    Workspace,
    Library,
    Devices,
    Tasks,
    Diagnostics,
    Dialog,
    Form
}

public sealed record KeyboardShortcut(
    string Id,
    string CommandId,
    string Gesture,
    string DescriptionKey,
    ShortcutScope Scope,
    bool IsUserConfigurable = true);

public sealed record ShortcutContext(
    ShortcutScope Scope,
    string? Route = null,
    string? FocusedElement = null);
