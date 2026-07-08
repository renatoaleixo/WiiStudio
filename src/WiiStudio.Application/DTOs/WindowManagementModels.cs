namespace WiiStudio.Application.DTOs;

public enum AppWindowState
{
    Normal,
    Maximized,
    Minimized,
    FullScreen,
    Closing,
    Restoring,
    InvalidPosition
}

public enum CloseDecision
{
    Allow,
    Cancel,
    AskConfirmation
}

public sealed record WindowStateSettings(
    double Width,
    double Height,
    double? X,
    double? Y,
    bool IsMaximized,
    string? DisplayId = null);
