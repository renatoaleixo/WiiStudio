using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record ExportRequest(
    IReadOnlyList<string> GameIds,
    string DestinationDirectory,
    FileFormat TargetFormat,
    ExportConflictResolution ConflictResolution = ExportConflictResolution.Cancel);
