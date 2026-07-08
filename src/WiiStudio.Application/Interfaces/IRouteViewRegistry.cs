namespace WiiStudio.Application.Interfaces;

public interface IRouteViewRegistry
{
    void Register(string route, Type viewModelType);

    bool TryResolve(string route, out Type viewModelType);
}
