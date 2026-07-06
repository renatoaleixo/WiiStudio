namespace WiiStudio.Application.DTOs;

public sealed record LibraryViewSnapshot(
    IReadOnlyList<GameDto> Games,
    int TotalCount,
    LibraryViewMode ViewMode,
    IReadOnlySet<string> SelectedGameIds,
    LibraryViewState State,
    DateTimeOffset RefreshedAt);
