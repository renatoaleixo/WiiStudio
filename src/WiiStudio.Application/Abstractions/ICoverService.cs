namespace WiiStudio.Application.Abstractions;

public interface ICoverService
{
    Task<Stream?> GetCoverAsync(string gameId, CancellationToken cancellationToken);
}
