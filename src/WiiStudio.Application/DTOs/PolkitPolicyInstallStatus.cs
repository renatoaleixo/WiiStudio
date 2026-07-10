namespace WiiStudio.Application.DTOs;

public sealed record PolkitPolicyInstallStatus(
    PolkitPolicyInstallState State,
    string? PolicyPath,
    string? MessageKey,
    string? RecommendedActionKey,
    string? DiagnosticDetails = null);
