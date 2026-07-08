namespace WiiStudio.Application.DTOs;

public enum TabKind
{
    Primary,
    Secondary,
    Vertical,
    Compact
}

public enum TabState
{
    Default,
    Active,
    Disabled,
    Loading,
    Error,
    Warning,
    Success
}

public sealed record TabDescriptor(
    string Id,
    string TitleKey,
    TabKind Kind,
    int Order,
    string? IconKey = null,
    string? BadgeText = null,
    TabState State = TabState.Default,
    bool IsLazyLoaded = false,
    bool IsPersistent = false);
