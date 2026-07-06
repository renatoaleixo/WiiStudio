using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Entities;

namespace WiiStudio.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly DetectUsbDrivesUseCase _detectUsbDrivesUseCase;
    private readonly IMainWindowService _mainWindowService;

    [ObservableProperty]
    private string _statusMessage = "Pronto";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ShellModule? _selectedModule;

    [ObservableProperty]
    private bool _isSidePanelVisible = true;

    [ObservableProperty]
    private MainWindowState _state = MainWindowState.Initializing;

    public MainViewModel(DetectUsbDrivesUseCase detectUsbDrivesUseCase, IMainWindowService mainWindowService)
    {
        _detectUsbDrivesUseCase = detectUsbDrivesUseCase;
        _mainWindowService = mainWindowService;

        foreach (var module in _mainWindowService.Modules)
        {
            Modules.Add(module);
        }
    }

    public ObservableCollection<ShellModule> Modules { get; } = new();

    public ObservableCollection<UsbDrive> Drives { get; } = new();

    public string SelectedModuleTitle => SelectedModule?.Title ?? "Dashboard";

    public string SelectedModuleDescription => SelectedModule?.Description ?? "Resumo do ambiente e atividades recentes.";

    public string TaskSummary => "0 tarefas ativas";

    public string NotificationSummary => "0 notificações";

    public string DeviceSummary => Drives.Count == 0 ? "Nenhum dispositivo" : $"{Drives.Count} dispositivo(s)";

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        State = MainWindowState.Loading;
        StatusMessage = "Carregando layout...";

        var snapshot = await _mainWindowService.InitializeAsync(cancellationToken);
        IsSidePanelVisible = snapshot.Layout.IsSidePanelVisible;
        SelectedModule = Modules.FirstOrDefault(module => module.Id == snapshot.Layout.ActiveModuleId)
            ?? Modules.FirstOrDefault();
        State = snapshot.State;
        StatusMessage = "Pronto";
    }

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
            OnPropertyChanged(nameof(DeviceSummary));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleSidePanelAsync(CancellationToken cancellationToken)
    {
        IsSidePanelVisible = !IsSidePanelVisible;
        await _mainWindowService.SetSidePanelVisibilityAsync(IsSidePanelVisible, cancellationToken);
        await _mainWindowService.SaveLayoutAsync(cancellationToken);
    }

    partial void OnSelectedModuleChanged(ShellModule? value)
    {
        OnPropertyChanged(nameof(SelectedModuleTitle));
        OnPropertyChanged(nameof(SelectedModuleDescription));

        if (value is null)
        {
            return;
        }

        _ = PersistSelectedModuleAsync(value.Id);
    }

    private async Task PersistSelectedModuleAsync(ShellModuleId moduleId)
    {
        await _mainWindowService.SelectModuleAsync(moduleId, CancellationToken.None);
        await _mainWindowService.SaveLayoutAsync(CancellationToken.None);
    }
}
