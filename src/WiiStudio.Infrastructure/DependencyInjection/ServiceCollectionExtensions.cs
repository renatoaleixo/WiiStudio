using Microsoft.Extensions.DependencyInjection;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Usb;
using WiiStudio.Infrastructure.Wiimms;

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
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IUsbDriveService, UsbDriveService>();
        services.AddSingleton<IWitService, WitService>();

        return services;
    }
}
