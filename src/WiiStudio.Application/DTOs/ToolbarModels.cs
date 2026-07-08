namespace WiiStudio.Application.DTOs;

public enum ToolbarKind
{
    Main,
    Context,
    Selection,
    Inline
}

public enum ToolbarCommandPriority
{
    Primary,
    Secondary,
    Tertiary,
    Destructive,
    Diagnostic
}

public sealed record ToolbarCommandDescriptor(
    string Id,
    string Title,
    string IconKey,
    ToolbarCommandPriority Priority,
    int Order,
    string? ShortcutText = null,
    bool IsEnabled = true,
    bool IsVisible = true);

public sealed record ToolbarDescriptor(
    ToolbarKind Kind,
    IReadOnlyList<ToolbarCommandDescriptor> Commands,
    bool IsOverflowEnabled,
    bool IsLoading);
