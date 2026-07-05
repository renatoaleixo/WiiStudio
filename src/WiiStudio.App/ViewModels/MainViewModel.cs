using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Entities;

namespace WiiStudio.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly DetectUsbDrivesUseCase _detectUsbDrivesUseCase;

    [ObservableProperty]
    private string _statusMessage = "Pronto";

    [ObservableProperty]
    private bool _isBusy;

    public MainViewModel(DetectUsbDrivesUseCase detectUsbDrivesUseCase)
    {
        _detectUsbDrivesUseCase = detectUsbDrivesUseCase;
    }

    public ObservableCollection<UsbDrive> Drives { get; } = new();

    [RelayCommand]
    private async Task DetectDrivesAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        StatusMessage = "Detectando dispositivos...";

        try
        {
            Drives.Clear();
            var drives = await _detectUsbDrivesUseCase.ExecuteAsync(cancellationToken);

            foreach (var drive in drives)
            {
                Drives.Add(drive);
            }

            StatusMessage = drives.Count == 0
                ? "Nenhum dispositivo encontrado."
                : $"{drives.Count} dispositivo(s) encontrado(s).";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
