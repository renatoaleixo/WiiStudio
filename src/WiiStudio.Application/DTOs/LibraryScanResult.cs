namespace WiiStudio.Application.DTOs;

public sealed record LibraryScanResult(
    IReadOnlyList<GameDto> Games,
    IReadOnlyList<string> DuplicateGameIds,
    DateTimeOffset ScannedAt)
{
    public int TotalGames => Games.Count;

    public bool HasDuplicates => DuplicateGameIds.Count > 0;
}
