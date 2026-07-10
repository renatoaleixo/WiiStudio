namespace WiiStudio.Application.DTOs;

public sealed record PolkitPolicyInstallResult(
    PolkitPolicyInstallState State,
    bool Success,
    string? PolicyPath,
    string? MessageKey,
    string? RecommendedActionKey,
    string? DiagnosticDetails = null);
