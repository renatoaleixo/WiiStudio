namespace WiiStudio.Application.DTOs;

public sealed record WiimmsOperationProgress(
    double? Percentage,
    string? CurrentOperation,
    string? CurrentFile,
    long? ProcessedBytes,
    long? TotalBytes,
    double? SpeedBytesPerSecond,
    TimeSpan? EstimatedRemaining,
    bool IsIndeterminate);
