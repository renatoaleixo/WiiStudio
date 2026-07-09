namespace WiiStudio.Application.DTOs;

public sealed record PrivilegedAccessStatus(
    OSPlatformKind Platform,
    PrivilegedAccessState State,
    bool CanRetry,
    bool RequiresUserAction,
    string? Message,
    string? RecommendedAction,
    string? DiagnosticDetails);
