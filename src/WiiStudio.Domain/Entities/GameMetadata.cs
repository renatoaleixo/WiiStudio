using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Entities;

public sealed class GameMetadata
{
    public GameMetadata(GameId gameId, string title, Region region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        GameId = gameId;
        Title = title.Trim();
        Region = region;
    }

    public GameId GameId { get; }

    public string Title { get; }

    public Region Region { get; }

    public string? Genre { get; init; }

    public string? Developer { get; init; }

    public int? Players { get; init; }
}
