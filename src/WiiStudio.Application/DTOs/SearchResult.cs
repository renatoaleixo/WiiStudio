namespace WiiStudio.Application.DTOs;

public sealed record SearchResult(
    IReadOnlyList<GameDto> Games,
    int TotalCount,
    DateTimeOffset SearchedAt);
