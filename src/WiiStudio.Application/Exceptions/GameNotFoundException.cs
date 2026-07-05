namespace WiiStudio.Application.Exceptions;

public sealed class GameNotFoundException : Exception
{
    public GameNotFoundException(string gameId)
        : base($"Game '{gameId}' was not found.")
    {
        GameId = gameId;
    }

    public string GameId { get; }
}
