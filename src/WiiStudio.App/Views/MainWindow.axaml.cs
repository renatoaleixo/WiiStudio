using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Selection;
using WiiStudio.App.ViewModels;

namespace WiiStudio.App.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        KeyDown += OnKeyDown;
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

    private async void OnLibrarySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel ||
            sender is not ListBox listBox ||
            listBox.SelectedItems is null)
        {
            return;
        }

        var selectedItems = listBox.SelectedItems
            .OfType<LibraryGameItem>()
            .ToArray();
        await viewModel.SetSelectedLibraryItemsAsync(selectedItems, CancellationToken.None);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        if (viewModel.IsEmulatorLaunchPreviewOpen)
        {
            viewModel.CancelEmulatorLaunchCommand.Execute(null);
            e.Handled = true;
        }
        else if (viewModel.IsCoverLightboxOpen)
        {
            viewModel.CloseCoverLightboxCommand.Execute(null);
            e.Handled = true;
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
