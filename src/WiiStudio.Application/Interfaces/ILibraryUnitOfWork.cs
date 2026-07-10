namespace WiiStudio.Application.Interfaces;

public interface ILibraryUnitOfWork
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
