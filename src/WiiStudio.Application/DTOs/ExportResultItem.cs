namespace WiiStudio.Application.DTOs;

public sealed record ExportResultItem(
    string GameId,
    ExportStatus Status,
    string DestinationPath,
    string? Message = null);
