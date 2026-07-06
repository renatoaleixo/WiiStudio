namespace WiiStudio.Application.DTOs;

public sealed record MainWindowSnapshot(
    MainWindowState State,
    MainWindowLayout Layout,
    IReadOnlyList<ShellModule> Modules,
    DateTimeOffset GeneratedAt);
