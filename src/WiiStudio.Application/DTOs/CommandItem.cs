namespace WiiStudio.Application.DTOs;

public sealed record CommandItem(
    string Id,
    string Title,
    CommandCategory Category,
    string Description,
    IReadOnlyList<string> Aliases,
    bool IsFavorite = false,
    int Score = 0);
