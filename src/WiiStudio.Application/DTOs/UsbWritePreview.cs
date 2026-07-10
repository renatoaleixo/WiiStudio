using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record UsbWritePreview(
    string SourcePath,
    string? GameId,
    Guid TargetDriveId,
    string TargetDeviceName,
    string TargetDevicePath,
    string TargetMountPoint,
    UsbDriveFileSystem TargetFileSystem,
    long SourceSizeBytes,
    long RequiredBytes,
    long FreeBytes,
    bool RequiresFat32Split,
    bool UserConfirmationRequired,
    bool IsBlocked,
    string? BlockReasonKey,
    DevicePermissionStatus? PermissionStatus,
    IReadOnlyList<string> Warnings);
