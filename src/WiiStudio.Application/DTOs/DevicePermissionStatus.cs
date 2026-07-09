namespace WiiStudio.Application.DTOs;

public sealed record DevicePermissionStatus(
    OSPlatformKind Platform,
    PrivilegedAccessState State,
    bool CanRetry,
    bool RequiresUserAction,
    string? MessageKey,
    string? RecommendedActionKey,
    string? DiagnosticDetails);
