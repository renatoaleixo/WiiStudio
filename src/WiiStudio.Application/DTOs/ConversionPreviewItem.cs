using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record ConversionPreviewItem(
    string SourcePath,
    string DestinationPath,
    FileFormat SourceFormat,
    FileFormat TargetFormat,
    string Pipeline,
    bool CanConvert,
    string Message);
