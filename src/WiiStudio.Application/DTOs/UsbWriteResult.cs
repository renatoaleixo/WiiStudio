namespace WiiStudio.Application.DTOs;

public sealed record UsbWriteResult(
    bool Success,
    string? GameId,
    Guid TargetDriveId,
    string? ErrorMessageKey,
    TimeSpan Duration);
