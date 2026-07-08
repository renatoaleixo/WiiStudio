namespace WiiStudio.Application.DTOs;

public enum CardState
{
    Default,
    Hover,
    Focus,
    Selected,
    Disabled,
    Loading,
    Empty,
    Error,
    Warning,
    Success
}

public enum CardKind
{
    Card,
    Container,
    Panel,
    Section,
    Tile,
    EmptyState,
    ErrorState,
    LoadingState
}

public sealed record CardDescriptor(
    CardKind Kind,
    string Title,
    string? Subtitle,
    string? IconKey,
    CardState State,
    string? PrimaryActionText = null);
