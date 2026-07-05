namespace WiiStudio.Application.DTOs;

public sealed record ExportPreviewItem(
    string GameId,
    string Title,
    string SourcePath,
    string DestinationPath,
    bool HasConflict,
    bool WillExport);
