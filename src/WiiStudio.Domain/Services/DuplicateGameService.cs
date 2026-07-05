using WiiStudio.Domain.Entities;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Services;

public sealed class DuplicateGameService
{
    public bool ContainsDuplicate(IEnumerable<Game> games, GameId gameId)
    {
        ArgumentNullException.ThrowIfNull(games);

        return games.Any(game => game.GameId == gameId);
    }
}
