namespace WiiStudio.Application.DTOs;

public sealed record UsbPartitionInfo(
    string PartitionId,
    string? DevicePath,
    string? MountPath,
    string? FileSystem,
    long? SizeBytes,
    long? FreeBytes,
    bool IsWbfsCandidate,
    bool IsWritable);
