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

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selecionar jogos Wii",
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

        var paths = files
            .Select(file => file.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .ToArray();

        if (paths.Length > 0)
        {
            await viewModel.SetImportSourcePathsAsync(paths, CancellationToken.None);
        }
    }
}
