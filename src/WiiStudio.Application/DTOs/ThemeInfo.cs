namespace WiiStudio.Application.DTOs;

public sealed record ThemeInfo(
    string Id,
    string Name,
    ThemeKind Kind,
    bool IsBuiltIn,
    bool IsActive,
    string? SourcePath = null);
