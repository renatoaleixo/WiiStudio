namespace WiiStudio.Infrastructure.Metadata;

public interface IGameTdbXmlParser
{
    Task<GameTdbGameXmlEntry?> FindByGameIdAsync(Stream xmlStream, string gameId, CancellationToken cancellationToken);
}
