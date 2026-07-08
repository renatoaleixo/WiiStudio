namespace WiiStudio.Application.DTOs;

public enum MenuKind
{
    Main,
    Context,
    Action,
    Flyout
}

public sealed record MenuItemDefinition(
    string Id,
    string TitleKey,
    string? IconKey,
    string? ShortcutText,
    int Order,
    bool IsEnabled = true,
    bool IsDanger = false,
    bool IsChecked = false,
    IReadOnlyList<MenuItemDefinition>? Children = null);

public sealed record MenuDefinition(
    MenuKind Kind,
    IReadOnlyList<MenuItemDefinition> Items);
