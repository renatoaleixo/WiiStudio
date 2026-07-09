namespace WiiStudio.Application.DTOs;

public enum PrivilegedAccessState
{
    Unknown,
    NotRequired,
    Required,
    Authorized,
    AuthorizationRequired,
    PolicyMissing,
    HelperMissing,
    PermissionDenied,
    Unsupported,
    Failed
}
