using Microsoft.Extensions.DependencyInjection;
using WiiStudio.Application.Abstractions;
using WiiStudio.Application.UseCases;
using WiiStudio.Infrastructure.Services;

namespace WiiStudio.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWiiStudioApplication(this IServiceCollection services)
    {
        services.AddTransient<DetectUsbDrivesUseCase>();
        services.AddTransient<ListGamesUseCase>();

        return services;
    }

    public static IServiceCollection AddWiiStudioInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IUsbDriveService, UsbDriveService>();
        services.AddSingleton<IWitService, WitService>();

        return services;
    }
}
