namespace WiiStudio.Application.DTOs;

public enum ReleaseValidationGateStatus
{
    Pending = 1,
    Passed = 2,
    Failed = 3,
    Blocked = 4,
    NotApplicable = 5
}

public sealed record ReleaseValidationRequest(
    string OutputDirectory,
    string Version,
    string? OperatorName = null,
    DateTimeOffset? CreatedAt = null);

public sealed record ReleaseValidationChecklistItem(
    string Id,
    string Title,
    string Category,
    ReleaseValidationGateStatus Status,
    bool RequiresHardware,
    string EvidenceHint);

public sealed record ReleaseValidationEvidenceFile(
    string Kind,
    string Path);

public sealed record ReleaseValidationPackage(
    string RootDirectory,
    string AcceptanceReportPath,
    IReadOnlyList<ReleaseValidationChecklistItem> Checklist,
    IReadOnlyList<ReleaseValidationEvidenceFile> Files,
    bool IsReleaseApproved);
