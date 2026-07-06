namespace WiiStudio.Application.DTOs;

public sealed record LogExportOptions(
    string OutputFile,
    LogQuery? Query = null);
