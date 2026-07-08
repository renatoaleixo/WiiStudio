namespace WiiStudio.Application.Interfaces;

public interface INavigationService
{
    bool CanGoBack { get; }

    bool CanGoForward { get; }

    string CurrentRoute { get; }

    Task NavigateToAsync(
        string route,
        object? parameter = null,
        CancellationToken cancellationToken = default);

    Task GoBackAsync(CancellationToken cancellationToken = default);

    Task GoForwardAsync(CancellationToken cancellationToken = default);
}
