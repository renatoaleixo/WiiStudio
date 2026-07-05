using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WiiStudio.App.ViewModels;
using WiiStudio.App.Views;
using WiiStudio.Infrastructure.DependencyInjection;

namespace WiiStudio.App;

public static class Program
{
    public static IServiceProvider Services { get; private set; } = default!;

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        using var host = CreateHostBuilder(args).Build();
        Services = host.Services;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddWiiStudioApplication();
                services.AddWiiStudioInfrastructure();
                services.AddSingleton<MainWindow>();
                services.AddTransient<MainViewModel>();
            });
    }
}
