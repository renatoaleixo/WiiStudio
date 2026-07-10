namespace WiiStudio.Application.DTOs;

public enum PolkitPolicyInstallState
{
    Unknown,
    NotLinux,
    Installed,
    Missing,
    Invalid,
    PkexecMissing,
    InstallRequiresAuthorization,
    InstallFailed,
    ValidationFailed
}
