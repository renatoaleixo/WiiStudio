namespace WiiStudio.Application.DTOs;

public enum FeedbackSeverity
{
    Info,
    Success,
    Warning,
    Error,
    Critical
}

public enum FeedbackKind
{
    Toast,
    Inline,
    Banner,
    Snackbar
}

public sealed record FeedbackMessage(
    string Id,
    FeedbackKind Kind,
    FeedbackSeverity Severity,
    string TitleKey,
    string MessageKey,
    string? ActionKey = null,
    bool IsPersistent = false,
    TimeSpan? Duration = null);
