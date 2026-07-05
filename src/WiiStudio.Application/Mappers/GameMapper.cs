using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Mappers;

public static class GameMapper
{
    public static GameDto ToDto(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        return new GameDto(
            game.GameId.Value,
            game.Title,
            game.Region,
            game.DiscType,
            game.Format,
            game.FilePath,
            game.SizeInBytes,
            game.AddedAt);
    }
}
