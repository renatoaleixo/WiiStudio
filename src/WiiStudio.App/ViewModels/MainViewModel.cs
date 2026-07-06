using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly DetectUsbDrivesUseCase _detectUsbDrivesUseCase;
    private readonly IConversionWizardService _conversionWizardService;
    private readonly IDashboardService _dashboardService;
    private readonly IDeviceManagerService _deviceManagerService;
    private readonly IImportWizardService _importWizardService;
    private readonly ILibraryViewService _libraryViewService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IStatusBarService _statusBarService;

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

    [ObservableProperty]
    private string? _librarySearchText;

    [ObservableProperty]
    private LibraryViewMode _selectedLibraryViewMode = LibraryViewMode.Detailed;

    [ObservableProperty]
    private LibraryViewState _libraryState = LibraryViewState.Loading;

    [ObservableProperty]
    private LibraryGameItem? _selectedGame;

    [ObservableProperty]
    private int _libraryTotalCount;

    [ObservableProperty]
    private int _selectedGameCount;

    [ObservableProperty]
    private string? _importSourcePathsText;

    [ObservableProperty]
    private UsbDrive? _selectedImportDrive;

    [ObservableProperty]
    private ImportWizardState _importWizardState = ImportWizardState.SelectingFiles;

    [ObservableProperty]
    private ImportWizardStep _selectedImportStep = ImportWizardStep.SelectingFiles;

    [ObservableProperty]
    private string _importSummary = "Nenhum arquivo analisado";

    [ObservableProperty]
    private bool _canExecuteImport;

    [ObservableProperty]
    private string? _conversionSourcePathsText;

    [ObservableProperty]
    private string? _conversionDestinationDirectory;

    [ObservableProperty]
    private FileFormat _selectedConversionTargetFormat = FileFormat.Wbfs;

    [ObservableProperty]
    private ConversionWizardState _conversionWizardState = ConversionWizardState.SelectingSource;

    [ObservableProperty]
    private ConversionWizardStep _selectedConversionStep = ConversionWizardStep.SelectingSource;

    [ObservableProperty]
    private string _conversionSummary = "Nenhuma conversão analisada";

    [ObservableProperty]
    private bool _canExecuteConversion;

    [ObservableProperty]
    private DeviceManagerState _deviceManagerState = DeviceManagerState.Detecting;

    [ObservableProperty]
    private DeviceInfoViewModel? _selectedDevice;

    [ObservableProperty]
    private string _deviceManagerSummary = "Nenhum dispositivo";

    [ObservableProperty]
    private DashboardState _dashboardState = DashboardState.Loading;

    [ObservableProperty]
    private string _dashboardUpdatedAt = "Nunca";

    [ObservableProperty]
    private StatusBarState _statusBarState = StatusBarState.Initializing;

    [ObservableProperty]
    private string _statusActiveDevice = "Nenhum dispositivo";

    [ObservableProperty]
    private string _statusFileSystem = "-";

    [ObservableProperty]
    private string _statusFreeSpace = "-";

    [ObservableProperty]
    private string _statusTasks = "0 tarefas";

    [ObservableProperty]
    private string _statusNotifications = "0 notificações";

    [ObservableProperty]
    private string _statusMemory = "-";

    [ObservableProperty]
    private string _statusVersion = "0.0.0";

    public MainViewModel(
        DetectUsbDrivesUseCase detectUsbDrivesUseCase,
        IConversionWizardService conversionWizardService,
        IDashboardService dashboardService,
        IDeviceManagerService deviceManagerService,
        IImportWizardService importWizardService,
        ILibraryViewService libraryViewService,
        IMainWindowService mainWindowService,
        IStatusBarService statusBarService)
    {
        _detectUsbDrivesUseCase = detectUsbDrivesUseCase;
        _conversionWizardService = conversionWizardService;
        _dashboardService = dashboardService;
        _deviceManagerService = deviceManagerService;
        _importWizardService = importWizardService;
        _libraryViewService = libraryViewService;
        _mainWindowService = mainWindowService;
        _statusBarService = statusBarService;

        foreach (var module in _mainWindowService.Modules)
        {
            Modules.Add(module);
        }
    }

    public ObservableCollection<ShellModule> Modules { get; } = new();

    public ObservableCollection<UsbDrive> Drives { get; } = new();

    public ObservableCollection<DeviceInfoViewModel> Devices { get; } = new();

    public ObservableCollection<DashboardWidgetViewModel> DashboardWidgets { get; } = new();

    public ObservableCollection<DashboardActivityViewModel> DashboardActivities { get; } = new();

    public ObservableCollection<DashboardQuickActionViewModel> DashboardQuickActions { get; } = new();

    public ObservableCollection<LibraryGameItem> Games { get; } = new();

    public ObservableCollection<ImportPreviewItemViewModel> ImportPreviewItems { get; } = new();

    public ObservableCollection<ConversionPreviewItemViewModel> ConversionPreviewItems { get; } = new();

    public IReadOnlyList<ImportWizardStep> ImportSteps { get; } =
    [
        ImportWizardStep.SelectingFiles,
        ImportWizardStep.Validating,
        ImportWizardStep.SelectingTarget,
        ImportWizardStep.Options,
        ImportWizardStep.Reviewing,
        ImportWizardStep.Importing,
        ImportWizardStep.Completed
    ];

    public IReadOnlyList<ConversionWizardStep> ConversionSteps { get; } =
    [
        ConversionWizardStep.SelectingSource,
        ConversionWizardStep.Analyzing,
        ConversionWizardStep.SelectingTarget,
        ConversionWizardStep.AdvancedOptions,
        ConversionWizardStep.Reviewing,
        ConversionWizardStep.Converting,
        ConversionWizardStep.Completed
    ];

    public IReadOnlyList<FileFormat> ConversionTargetFormats { get; } =
    [
        FileFormat.Iso,
        FileFormat.Wbfs
    ];

    public IReadOnlyList<LibraryViewMode> LibraryViewModes { get; } =
    [
        LibraryViewMode.Grid,
        LibraryViewMode.List,
        LibraryViewMode.Compact,
        LibraryViewMode.Detailed
    ];

    public string SelectedModuleTitle => SelectedModule?.Title ?? "Dashboard";

    public string SelectedModuleDescription => SelectedModule?.Description ?? "Resumo do ambiente e atividades recentes.";

    public string LibrarySummary => LibraryTotalCount == 1 ? "1 jogo" : $"{LibraryTotalCount} jogos";

    public string SelectionSummary => SelectedGameCount == 1 ? "1 selecionado" : $"{SelectedGameCount} selecionados";

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

        await RefreshDevicesAsync(cancellationToken);
        await RefreshLibraryAsync(cancellationToken);
        await RefreshDashboardAsync(cancellationToken);
        await RefreshStatusBarAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task RefreshLibraryAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        LibraryState = string.IsNullOrWhiteSpace(LibrarySearchText)
            ? LibraryViewState.Refreshing
            : LibraryViewState.Searching;
        StatusMessage = "Carregando biblioteca...";

        try
        {
            var snapshot = await _libraryViewService.RefreshAsync(
                new SearchRequest(SearchText: LibrarySearchText),
                cancellationToken);

            ApplyLibrarySnapshot(snapshot);
            StatusMessage = snapshot.TotalCount == 0
                ? "Biblioteca vazia."
                : $"{snapshot.TotalCount} jogo(s) carregado(s).";
            await RefreshStatusBarAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync(CancellationToken cancellationToken)
    {
        DashboardState = DashboardState.Refreshing;
        StatusMessage = "Atualizando dashboard...";

        var data = await _dashboardService.RefreshAsync(cancellationToken);
        ApplyDashboard(data);

        DashboardState = DashboardState.Ready;
        StatusMessage = "Dashboard atualizado.";
        await RefreshStatusBarAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task RefreshStatusBarAsync(CancellationToken cancellationToken)
    {
        StatusBarState = StatusBarState.Updating;
        var info = await _statusBarService.RefreshAsync(cancellationToken);
        ApplyStatusBar(info);
    }

    [RelayCommand]
    private Task ClearLibrarySearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(LibrarySearchText))
        {
            return RefreshLibraryAsync(cancellationToken);
        }

        LibrarySearchText = null;
        return Task.CompletedTask;
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

            SelectedImportDrive ??= Drives.FirstOrDefault();

            StatusMessage = drives.Count == 0
                ? "Nenhum dispositivo encontrado."
                : $"{drives.Count} dispositivo(s) encontrado(s).";
            OnPropertyChanged(nameof(DeviceSummary));
            await RefreshStatusBarAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDevicesAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        DeviceManagerState = DeviceManagerState.Refreshing;
        StatusMessage = "Atualizando dispositivos...";

        try
        {
            var snapshot = await _deviceManagerService.RefreshAsync(cancellationToken);
            ApplyDeviceSnapshot(snapshot);
            StatusMessage = snapshot.Devices.Count == 0
                ? "Nenhum dispositivo encontrado."
                : $"{snapshot.Devices.Count} dispositivo(s) atualizado(s).";
            await RefreshStatusBarAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EjectSelectedDeviceAsync(CancellationToken cancellationToken)
    {
        if (SelectedDevice is null)
        {
            StatusMessage = "Selecione um dispositivo.";
            return;
        }

        DeviceManagerState = DeviceManagerState.Busy;
        StatusMessage = "Ejetando dispositivo...";

        var ejected = await _deviceManagerService.EjectAsync(SelectedDevice.DeviceId, cancellationToken);
        ReplaceDevice(ejected);
        DeviceManagerState = DeviceManagerState.Ready;
        StatusMessage = "Dispositivo marcado para ejeção segura.";
        await RefreshStatusBarAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task AnalyzeImportAsync(CancellationToken cancellationToken)
    {
        ImportWizardState = ImportWizardState.Validating;
        SelectedImportStep = ImportWizardStep.Validating;
        StatusMessage = "Validando arquivos para importação...";

        var sourcePaths = ParseImportSourcePaths();
        var preview = await _importWizardService.AnalyzeAsync(sourcePaths, cancellationToken);

        ImportPreviewItems.Clear();
        foreach (var item in preview.Items)
        {
            ImportPreviewItems.Add(new ImportPreviewItemViewModel(item));
        }

        ImportWizardState = preview.State;
        SelectedImportStep = preview.CanImport ? ImportWizardStep.SelectingTarget : ImportWizardStep.Validating;
        CanExecuteImport = preview.CanImport && SelectedImportDrive is not null;
        ImportSummary = preview.Items.Count == 0
            ? "Informe ao menos um arquivo."
            : $"{preview.Items.Count} arquivo(s) analisado(s).";
        StatusMessage = preview.CanImport ? "Arquivos prontos para importação." : "Revise os arquivos informados.";
    }

    [RelayCommand]
    private async Task ExecuteImportAsync(CancellationToken cancellationToken)
    {
        if (!CanExecuteImport || SelectedImportDrive is null)
        {
            StatusMessage = "Selecione arquivos válidos e um destino.";
            return;
        }

        IsBusy = true;
        ImportWizardState = ImportWizardState.Importing;
        SelectedImportStep = ImportWizardStep.Importing;
        StatusMessage = "Importando jogos...";

        try
        {
            var result = await _importWizardService.ExecuteAsync(
                new ImportWizardRequest(ParseImportSourcePaths(), SelectedImportDrive.Id),
                cancellationToken);

            ImportWizardState = result.State;
            SelectedImportStep = ImportWizardStep.Completed;
            ImportSummary = result.Message;
            StatusMessage = $"{result.ImportedCount} jogo(s) importado(s).";
            await RefreshLibraryAsync(cancellationToken);
            await RefreshStatusBarAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            ImportWizardState = ImportWizardState.Failed;
            SelectedImportStep = ImportWizardStep.Reviewing;
            ImportSummary = exception.Message;
            StatusMessage = "Falha ao importar jogos.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelImport()
    {
        ImportWizardState = ImportWizardState.Cancelled;
        SelectedImportStep = ImportWizardStep.SelectingFiles;
        ImportPreviewItems.Clear();
        CanExecuteImport = false;
        ImportSummary = "Importação cancelada";
        StatusMessage = "Importação cancelada.";
    }

    [RelayCommand]
    private async Task AnalyzeConversionAsync(CancellationToken cancellationToken)
    {
        ConversionWizardState = ConversionWizardState.Analyzing;
        SelectedConversionStep = ConversionWizardStep.Analyzing;
        StatusMessage = "Analisando conversões...";

        var preview = await _conversionWizardService.AnalyzeAsync(CreateConversionRequest(), cancellationToken);

        ConversionPreviewItems.Clear();
        foreach (var item in preview.Items)
        {
            ConversionPreviewItems.Add(new ConversionPreviewItemViewModel(item));
        }

        ConversionWizardState = preview.State;
        SelectedConversionStep = preview.CanConvert ? ConversionWizardStep.Reviewing : ConversionWizardStep.Analyzing;
        CanExecuteConversion = preview.CanConvert;
        ConversionSummary = preview.Items.Count == 0
            ? "Informe ao menos uma origem."
            : $"{preview.Items.Count} item(ns), ETA {preview.EstimatedDuration.TotalMinutes:0} min.";
        StatusMessage = preview.CanConvert ? "Conversões prontas." : "Revise as conversões incompatíveis.";
    }

    [RelayCommand]
    private async Task ExecuteConversionAsync(CancellationToken cancellationToken)
    {
        if (!CanExecuteConversion)
        {
            StatusMessage = "Analise conversões válidas antes de executar.";
            return;
        }

        IsBusy = true;
        ConversionWizardState = ConversionWizardState.Converting;
        SelectedConversionStep = ConversionWizardStep.Converting;
        StatusMessage = "Convertendo jogos...";

        try
        {
            var result = await _conversionWizardService.ExecuteAsync(CreateConversionRequest(), cancellationToken);
            ConversionWizardState = result.State;
            SelectedConversionStep = result.State == ConversionWizardState.Completed
                ? ConversionWizardStep.Completed
                : ConversionWizardStep.Reviewing;
            ConversionSummary = result.Message;
            StatusMessage = $"{result.Jobs.Count} conversão(ões) processada(s).";
            await RefreshLibraryAsync(cancellationToken);
            await RefreshStatusBarAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            ConversionWizardState = ConversionWizardState.Failed;
            SelectedConversionStep = ConversionWizardStep.Reviewing;
            ConversionSummary = exception.Message;
            StatusMessage = "Falha ao converter jogos.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelConversion()
    {
        ConversionWizardState = ConversionWizardState.Cancelled;
        SelectedConversionStep = ConversionWizardStep.SelectingSource;
        ConversionPreviewItems.Clear();
        CanExecuteConversion = false;
        ConversionSummary = "Conversão cancelada";
        StatusMessage = "Conversão cancelada.";
    }

    [RelayCommand]
    private async Task RunDashboardQuickActionAsync(DashboardQuickActionViewModel? action)
    {
        if (action is null)
        {
            return;
        }

        switch (action.Id)
        {
            case "scan-library":
                await RefreshLibraryAsync(CancellationToken.None);
                break;
            case "detect-devices":
                await RefreshDevicesAsync(CancellationToken.None);
                break;
            case "import-games":
                SelectedImportStep = ImportWizardStep.SelectingFiles;
                StatusMessage = "Fluxo de importação pronto.";
                break;
            case "convert-games":
                SelectedConversionStep = ConversionWizardStep.SelectingSource;
                StatusMessage = "Fluxo de conversão pronto.";
                break;
        }
    }

    [RelayCommand]
    private async Task ToggleSidePanelAsync(CancellationToken cancellationToken)
    {
        IsSidePanelVisible = !IsSidePanelVisible;
        await _mainWindowService.SetSidePanelVisibilityAsync(IsSidePanelVisible, cancellationToken);
        await _mainWindowService.SaveLayoutAsync(cancellationToken);
    }

    partial void OnLibrarySearchTextChanged(string? value)
    {
        _ = RefreshLibraryAsync(CancellationToken.None);
    }

    partial void OnSelectedLibraryViewModeChanged(LibraryViewMode value)
    {
        _ = ChangeLibraryViewModeAsync(value);
    }

    partial void OnSelectedGameChanged(LibraryGameItem? value)
    {
        _ = SelectVisibleGamesAsync(value);
    }

    partial void OnSelectedImportDriveChanged(UsbDrive? value)
    {
        CanExecuteImport = value is not null
            && ImportPreviewItems.Count > 0
            && ImportPreviewItems.All(item => item.Status != ImportWizardFileStatus.Error.ToString());
    }

    partial void OnSelectedDeviceChanged(DeviceInfoViewModel? value)
    {
        DeviceManagerSummary = value is null
            ? DeviceSummary
            : $"{value.Name} · {value.FileSystem} · {value.FreeSpace} livre";
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

    private async Task ChangeLibraryViewModeAsync(LibraryViewMode mode)
    {
        var snapshot = await _libraryViewService.ChangeViewModeAsync(mode, CancellationToken.None);
        ApplyLibrarySnapshot(snapshot, refreshGames: false);
    }

    private async Task SelectVisibleGamesAsync(LibraryGameItem? game)
    {
        IReadOnlyList<string> selectedIds = game is null ? [] : [game.Id];
        var snapshot = await _libraryViewService.SelectGamesAsync(selectedIds, CancellationToken.None);
        ApplyLibrarySnapshot(snapshot, refreshGames: false);
    }

    private void ApplyLibrarySnapshot(LibraryViewSnapshot snapshot, bool refreshGames = true)
    {
        LibraryState = snapshot.State;
        LibraryTotalCount = snapshot.TotalCount;
        SelectedLibraryViewMode = snapshot.ViewMode;
        SelectedGameCount = snapshot.SelectedGameIds.Count;

        if (refreshGames)
        {
            Games.Clear();
            foreach (var game in snapshot.Games)
            {
                Games.Add(new LibraryGameItem(game));
            }
        }

        OnPropertyChanged(nameof(LibrarySummary));
        OnPropertyChanged(nameof(SelectionSummary));
    }

    private void ApplyDashboard(DashboardData data)
    {
        DashboardWidgets.Clear();
        foreach (var widget in data.Widgets)
        {
            DashboardWidgets.Add(new DashboardWidgetViewModel(widget));
        }

        DashboardActivities.Clear();
        foreach (var activity in data.Activities)
        {
            DashboardActivities.Add(new DashboardActivityViewModel(activity));
        }

        DashboardQuickActions.Clear();
        foreach (var action in data.QuickActions)
        {
            DashboardQuickActions.Add(new DashboardQuickActionViewModel(action));
        }

        DashboardUpdatedAt = data.LoadedAt.ToLocalTime().ToString("dd/MM HH:mm");
    }

    private void ApplyStatusBar(StatusBarInfo info)
    {
        StatusBarState = info.State;
        StatusActiveDevice = info.ActiveDevice;
        StatusFileSystem = info.FileSystem;
        StatusFreeSpace = info.FreeSpace;
        StatusTasks = info.ActiveTasks == 1 ? "1 tarefa" : $"{info.ActiveTasks} tarefas";
        StatusNotifications = info.PendingNotifications == 1
            ? "1 notificação"
            : $"{info.PendingNotifications} notificações";
        StatusMemory = info.MemoryUsage;
        StatusVersion = info.Version;
    }

    private IReadOnlyList<string> ParseImportSourcePaths()
    {
        if (string.IsNullOrWhiteSpace(ImportSourcePathsText))
        {
            return [];
        }

        return ImportSourcePathsText
            .Split(['\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void ApplyDeviceSnapshot(DeviceManagerSnapshot snapshot)
    {
        DeviceManagerState = snapshot.State;
        Devices.Clear();

        foreach (var device in snapshot.Devices)
        {
            Devices.Add(new DeviceInfoViewModel(device));
        }

        SelectedDevice = Devices.FirstOrDefault();
        DeviceManagerSummary = Devices.Count == 0
            ? "Nenhum dispositivo"
            : $"{Devices.Count} dispositivo(s)";
    }

    private void ReplaceDevice(DeviceInfo device)
    {
        var replacement = new DeviceInfoViewModel(device);
        var index = Devices
            .Select((item, itemIndex) => new { item, itemIndex })
            .FirstOrDefault(candidate => candidate.item.DeviceId == device.DeviceId)
            ?.itemIndex;

        if (index is null)
        {
            Devices.Add(replacement);
        }
        else
        {
            Devices[index.Value] = replacement;
        }

        SelectedDevice = replacement;
        DeviceManagerSummary = $"{replacement.Name} · {replacement.Status}";
    }

    private ConversionWizardRequest CreateConversionRequest()
    {
        return new ConversionWizardRequest(
            ParseSourcePaths(ConversionSourcePathsText),
            ConversionDestinationDirectory ?? string.Empty,
            SelectedConversionTargetFormat);
    }

    private static IReadOnlyList<string> ParseSourcePaths(string? sourcePathsText)
    {
        if (string.IsNullOrWhiteSpace(sourcePathsText))
        {
            return [];
        }

        return sourcePathsText
            .Split(['\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
