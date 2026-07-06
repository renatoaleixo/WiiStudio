using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Application.UseCases;
using WiiStudio.Application.Validators;
using WiiStudio.Infrastructure.Covers;
using WiiStudio.Infrastructure.Diagnostics;
using WiiStudio.Infrastructure.Export;
using WiiStudio.Infrastructure.Favorites;
using WiiStudio.Infrastructure.GameTdb;
using WiiStudio.Infrastructure.Metadata;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Settings;
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
        services.AddSingleton<IExportGameService, ExportGameService>();
        services.AddSingleton<IVerifyGameService, VerifyGameService>();
        services.AddSingleton<IRepairGameService, RepairGameService>();
        services.AddSingleton<IAssetManager, AssetManager>();
        services.AddSingleton<IBannerAssetService, BannerAssetService>();
        services.AddSingleton<ICoverService, CoverService>();
        services.AddSingleton<ICoverDownloadService, CoverDownloadService>();
        services.AddSingleton<IMetadataSynchronizationService, MetadataSynchronizationService>();
        services.AddSingleton<IGameTdbUpdateService, GameTdbUpdateService>();
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
        services.AddSingleton(new CoverCacheOptions());
        services.AddSingleton(new MetadataCacheOptions());
        services.AddSingleton(new GameTdbUpdateStateStoreOptions());
        services.AddSingleton(new SettingsStoreOptions());
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IExportFileSystem, PhysicalExportFileSystem>();
        services.AddSingleton<IFavoriteGameStore, JsonFavoriteGameStore>();
        services.AddSingleton<ISettingsService, JsonSettingsService>();
        services.AddSingleton<ICoverCache, FileSystemCoverCache>();
        services.AddSingleton<ICoverProvider, NullCoverProvider>();
        services.AddSingleton<IMetadataCacheService, JsonMetadataCacheService>();
        services.AddSingleton<IGameMetadataProvider, NullGameMetadataProvider>();
        services.AddSingleton<IGameTdbUpdateSource, NullGameTdbUpdateSource>();
        services.AddSingleton<IGameTdbUpdateStateStore, JsonGameTdbUpdateStateStore>();
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
