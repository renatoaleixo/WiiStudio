namespace WiiStudio.Application.DTOs;

public sealed record PolkitPolicyValidationResult(
    bool IsValid,
    PolkitPolicyInstallState State,
    string PolicyPath,
    bool HasExpectedAction,
    bool HasAuthAdminKeep,
    bool HasExpectedVendor,
    IReadOnlyList<string> Errors);
