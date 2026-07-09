using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using WiiStudio.App.ViewModels;

namespace WiiStudio.App.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.InitializeAsync(CancellationToken.None);
        }
    }

    private async void OnPickImportFilesClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || StorageProvider is null)
        {
            return;
        }

        var paths = await PickGameFilesAsync("Selecionar jogos Wii");

        if (paths.Length > 0)
        {
            await viewModel.SetImportSourcePathsAsync(paths, CancellationToken.None);
        }
    }

    private async void OnPickConversionFilesClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || StorageProvider is null)
        {
            return;
        }

        var paths = await PickGameFilesAsync("Selecionar jogos para conversão");

        if (paths.Length > 0)
        {
            await viewModel.SetConversionSourcePathsAsync(paths, CancellationToken.None);
        }
    }

    private async Task<string[]> PickGameFilesAsync(string title)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Jogos Wii")
                {
                    Patterns = ["*.iso", "*.wbfs", "*.rvz", "*.nkit.iso"]
                },
                FilePickerFileTypes.All
            ]
        });

        return files
            .Select(file => file.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .ToArray();
    }
}
