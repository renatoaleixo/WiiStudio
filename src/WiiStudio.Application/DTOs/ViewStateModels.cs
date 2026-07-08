namespace WiiStudio.Application.DTOs;

public enum ViewState
{
    Idle,
    Loading,
    Empty,
    NoResults,
    Ready,
    Error,
    PartialError
}

public sealed record StateDescriptor(
    ViewState State,
    string TitleKey,
    string MessageKey,
    string? IconKey = null,
    string? PrimaryActionKey = null,
    string? SecondaryActionKey = null);
