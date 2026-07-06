namespace WiiStudio.Application.DTOs;

public sealed record ShellModule(
    ShellModuleId Id,
    string Title,
    string Description,
    bool IsAvailable = true);
