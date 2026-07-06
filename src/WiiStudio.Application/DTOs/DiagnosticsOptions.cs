namespace WiiStudio.Application.DTOs;

public sealed record DiagnosticsOptions(
    bool IncludeSystem = true,
    bool IncludeDependencies = true,
    bool IncludeStorage = true,
    bool IncludeConfiguration = true,
    bool IncludeDevices = true);
