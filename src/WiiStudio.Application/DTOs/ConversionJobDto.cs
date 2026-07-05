using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record ConversionJobDto(
    Guid Id,
    string SourcePath,
    string DestinationPath,
    FileFormat SourceFormat,
    FileFormat TargetFormat,
    ConversionJobStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? FinishedAt,
    string? ErrorMessage);
