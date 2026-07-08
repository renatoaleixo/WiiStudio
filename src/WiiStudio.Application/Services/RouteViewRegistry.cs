using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class RouteViewRegistry : IRouteViewRegistry
{
    private readonly Dictionary<string, Type> _routes = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string route, Type viewModelType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        _routes[route] = viewModelType;
    }

    public bool TryResolve(string route, out Type viewModelType)
    {
        return _routes.TryGetValue(route, out viewModelType!);
    }
}
