using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Commands;

public sealed record ConvertGameCommand(string SourcePath, string DestinationPath, FileFormat TargetFormat);
