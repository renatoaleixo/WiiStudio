namespace WiiStudio.Application.DTOs;

public sealed record ApplicationUpdatePackage(
    ApplicationUpdateInfo Info,
    string PackagePath,
    string? Signature = null,
    string? Checksum = null);
