using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record ConversionWizardRequest(
    IReadOnlyList<string> SourcePaths,
    string DestinationDirectory,
    FileFormat TargetFormat);
