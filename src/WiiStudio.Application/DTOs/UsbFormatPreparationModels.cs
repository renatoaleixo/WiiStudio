namespace WiiStudio.Application.DTOs;

public enum UsbPreparationMode
{
    PrepareFat32WbfsFolder,
    FormatAsWbfs
}

public enum UsbFormatWorkflowState
{
    SelectingTarget,
    ValidatingTarget,
    PreviewReady,
    Blocked,
    WaitingForConfirmation,
    WaitingForAuthorization,
    Running,
    ValidatingResult,
    Completed,
    Failed,
    Cancelled
}

public sealed record UsbFormatPreparationRequest(
    string TargetDeviceId,
    string? TargetPartitionId,
    UsbPreparationMode Mode);

public sealed record UsbFormatPreview(
    string TargetDeviceId,
    string? TargetPartitionId,
    string TargetDisplayName,
    string? PhysicalPath,
    string? PartitionPath,
    string? MountPath,
    string? CurrentFileSystem,
    long? SizeBytes,
    UsbPreparationMode Mode,
    bool IsDestructive,
    bool IsBlocked,
    string? BlockReasonKey,
    string RequiredConfirmationPhrase,
    DevicePermissionStatus? PermissionStatus,
    IReadOnlyList<string> Warnings);

public sealed record UsbFormatExecutionRequest(
    UsbFormatPreview Preview,
    string ConfirmationPhrase,
    bool UserConfirmed);

public sealed record UsbFormatProgress(
    double? Percentage,
    UsbFormatWorkflowState State,
    string Message,
    bool IsIndeterminate);

public sealed record UsbFormatPreparationResult(
    bool Success,
    UsbPreparationMode Mode,
    string TargetDeviceId,
    string? ResultFileSystem,
    string? ErrorMessageKey,
    bool PostFormatDiagnosticsPassed);
