namespace WiiStudio.Infrastructure.Wiimms;

public interface IWiimmsToolLocator
{
    Task<string> FindWitAsync(CancellationToken cancellationToken);

    Task<string> FindWwtAsync(CancellationToken cancellationToken);
}
