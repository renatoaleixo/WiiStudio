namespace WiiStudio.Application.DTOs;

public sealed record UsbWriteExecutionRequest(
    UsbWritePreview Preview,
    bool UserConfirmed);
