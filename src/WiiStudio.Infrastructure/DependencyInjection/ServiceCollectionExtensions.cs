using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Application.UseCases;
using WiiStudio.Application.Validators;
using WiiStudio.Infrastructure.Diagnostics;
using WiiStudio.Infrastructure.Favorites;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Usb;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWiiStudioApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<ImportGameCommand>, ImportGameCommandValidator>();
        services.AddSingleton<IValidator<RemoveGameCommand>, RemoveGameCommandValidator>();
        services.AddTransient<IGameLibraryScanner, GameLibraryScanner>();
        services.AddSingleton<IGameLibraryService, GameLibraryService>();
        services.AddSingleton<IFavoriteGameService, FavoriteGameService>();
        services.AddSingleton<IGameDetailsService, GameDetailsService>();
        services.AddSingleton<IGameSearchService, GameSearchService>();
        services.AddSingleton<ISearchPresetService, SearchPresetService>();
        services.AddTransient<DetectUsbDrivesUseCase>();
        services.AddTransient<ScanUsbDrivesUseCase>();
        services.AddTransient<ScanGameLibraryUseCase>();
        services.AddTransient<ListGamesUseCase>();
        services.AddTransient<ImportGamesUseCase>();
        services.AddTransient<RemoveGameUseCase>();

        return services;
    }

    public static IServiceCollection AddWiiStudioInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(new WiimmsToolOptions());
        services.AddSingleton(new FavoriteGameStoreOptions());
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IFavoriteGameStore, JsonFavoriteGameStore>();
        services.AddSingleton<IWiimmsToolLocator, WiimmsToolLocator>();
        services.AddSingleton<IUsbProvider, LinuxUsbProvider>();
        services.AddSingleton<IUsbDetectionService, UsbDetectionService>();
        services.AddSingleton<IUsbDriveService, UsbDriveService>();
        services.AddSingleton<IWitService, WitService>();
        services.AddSingleton<IWwtService, WwtService>();
        services.AddSingleton<IDiagnosticsService, DiagnosticsService>();

        return services;
    }
}
