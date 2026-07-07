namespace WiiStudio.Application.DTOs;

public sealed record ThemeSnapshot(
    IReadOnlyList<ThemeInfo> Themes,
    ThemeInfo ActiveTheme,
    ThemeEngineState State,
    DateTimeOffset RefreshedAt);
