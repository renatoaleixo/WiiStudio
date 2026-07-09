namespace WiiStudio.Infrastructure.Metadata;

public interface IGameTdbDatabaseService
{
    Task<GameTdbDatabaseStatus> EnsureDatabaseAsync(bool forceRefresh, CancellationToken cancellationToken);

    Task<Stream> OpenDatabaseXmlAsync(CancellationToken cancellationToken);
}
