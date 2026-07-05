using Microsoft.Extensions.DependencyInjection;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Application.UseCases;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Usb;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWiiStudioApplication(this IServiceCollection services)
    {
        services.AddTransient<IGameLibraryScanner, GameLibraryScanner>();
        services.AddTransient<DetectUsbDrivesUseCase>();
        services.AddTransient<ScanUsbDrivesUseCase>();
        services.AddTransient<ScanGameLibraryUseCase>();
        services.AddTransient<ListGamesUseCase>();

        return services;
    }

    public static IServiceCollection AddWiiStudioInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IUsbProvider, LinuxUsbProvider>();
        services.AddSingleton<IUsbDetectionService, UsbDetectionService>();
        services.AddSingleton<IUsbDriveService, UsbDriveService>();
        services.AddSingleton<IWitService, WitService>();

        return services;
    }
}
