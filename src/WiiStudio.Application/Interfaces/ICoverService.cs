namespace WiiStudio.Application.Interfaces;

public interface ICoverService
{
    Task<Stream?> GetCoverAsync(string gameId, CancellationToken cancellationToken);
}
