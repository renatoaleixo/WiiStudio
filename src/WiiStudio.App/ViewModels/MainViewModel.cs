using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Plugin.Abstractions;
using WiiStudio.Plugin.Hosting;
using AvaloniaApp = Avalonia.Application;

namespace WiiStudio.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ICommandPaletteService _commandPaletteService;
    private readonly DetectUsbDrivesUseCase _detectUsbDrivesUseCase;
    private readonly IConversionWizardService _conversionWizardService;
    private readonly ICoverDownloadService _coverDownloadService;
    private readonly IDashboardService _dashboardService;
    private readonly IDeviceManagerService _deviceManagerService;
    private readonly IDiagnosticsService _diagnosticsService;
    private readonly IDockService _dockService;
    private readonly IImportWizardService _importWizardService;
    private readonly ILibraryViewService _libraryViewService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IMetadataSynchronizationService _metadataSynchronizationService;
    private readonly IPluginDiscoveryService _pluginDiscoveryService;
    private readonly IPluginLoaderService _pluginLoaderService;
    private readonly IPluginRegistry _pluginRegistry;
    private readonly RemoveGameUseCase _removeGameUseCase;
    private readonly IStatusBarService _statusBarService;
    private readonly IThemeService _themeService;
    private IReadOnlySet<string> _selectedGameIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

    [ObservableProperty]
    private string _dockLayoutName = "Default";

    [ObservableProperty]
    private string? _selectedDockLayout;

    [ObservableProperty]
    private string _dockLayoutSummary = "Layout Default";

    [ObservableProperty]
    private ThemeEngineState _themeEngineState = ThemeEngineState.Loading;

    [ObservableProperty]
    private ThemeOptionViewModel? _selectedTheme;

    [ObservableProperty]
    private string _themeSummary = "Tema do sistema";

    [ObservableProperty]
    private CommandPaletteState _commandPaletteState = CommandPaletteState.Hidden;

    [ObservableProperty]
    private bool _isCommandPaletteOpen;

    [ObservableProperty]
    private string? _commandPaletteQuery;

    [ObservableProperty]
    private CommandPaletteItemViewModel? _selectedCommandPaletteItem;

    public MainViewModel(
        ICommandPaletteService commandPaletteService,
        DetectUsbDrivesUseCase detectUsbDrivesUseCase,
        IConversionWizardService conversionWizardService,
        ICoverDownloadService coverDownloadService,
        IDashboardService dashboardService,
        IDeviceManagerService deviceManagerService,
        IDiagnosticsService diagnosticsService,
        IDockService dockService,
        IImportWizardService importWizardService,
        ILibraryViewService libraryViewService,
        IMainWindowService mainWindowService,
        IMetadataSynchronizationService metadataSynchronizationService,
        IPluginDiscoveryService pluginDiscoveryService,
        IPluginLoaderService pluginLoaderService,
        IPluginRegistry pluginRegistry,
        RemoveGameUseCase removeGameUseCase,
        IStatusBarService statusBarService,
        ShellCommandExecutor shellCommandExecutor,
        IThemeService themeService)
    {
        _commandPaletteService = commandPaletteService;
        _detectUsbDrivesUseCase = detectUsbDrivesUseCase;
        _conversionWizardService = conversionWizardService;
        _coverDownloadService = coverDownloadService;
        _dashboardService = dashboardService;
        _deviceManagerService = deviceManagerService;
        _diagnosticsService = diagnosticsService;
        _dockService = dockService;
        _importWizardService = importWizardService;
        _libraryViewService = libraryViewService;
        _mainWindowService = mainWindowService;
        _metadataSynchronizationService = metadataSynchronizationService;
        _pluginDiscoveryService = pluginDiscoveryService;
        _pluginLoaderService = pluginLoaderService;
        _pluginRegistry = pluginRegistry;
        _removeGameUseCase = removeGameUseCase;
        _statusBarService = statusBarService;
        _themeService = themeService;
        shellCommandExecutor.SetHandler(HandleShellCommandAsync);

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

    public ObservableCollection<string> DockLayouts { get; } = new();

    public ObservableCollection<ThemeOptionViewModel> Themes { get; } = new();

    public ObservableCollection<CommandPaletteItemViewModel> CommandPaletteItems { get; } = new();

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
        await RefreshDockLayoutsAsync(cancellationToken);
        await RefreshThemesAsync(cancellationToken);
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
    private async Task RefreshDockLayoutsAsync(CancellationToken cancellationToken)
    {
        var layouts = await _dockService.GetLayoutsAsync(cancellationToken);
        DockLayouts.Clear();

        foreach (var layout in layouts)
        {
            DockLayouts.Add(layout);
        }

        SelectedDockLayout ??= DockLayouts.FirstOrDefault();
        DockLayoutSummary = $"{DockLayouts.Count} layout(s)";
    }

    [RelayCommand]
    private async Task SaveDockLayoutAsync(CancellationToken cancellationToken)
    {
        var layoutName = string.IsNullOrWhiteSpace(DockLayoutName) ? "Default" : DockLayoutName;
        await _dockService.SaveLayoutAsync(layoutName, cancellationToken);
        DockLayoutName = layoutName;
        SelectedDockLayout = layoutName;
        DockLayoutSummary = $"Layout {layoutName} salvo.";
        await RefreshDockLayoutsAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task LoadDockLayoutAsync(CancellationToken cancellationToken)
    {
        var layoutName = SelectedDockLayout ?? DockLayoutName;
        if (string.IsNullOrWhiteSpace(layoutName))
        {
            return;
        }

        await _dockService.LoadLayoutAsync(layoutName, cancellationToken);
        DockLayoutName = layoutName;
        DockLayoutSummary = $"Layout {layoutName} carregado.";
    }

    [RelayCommand]
    private async Task ResetDockLayoutAsync(CancellationToken cancellationToken)
    {
        await _dockService.ResetLayoutAsync(cancellationToken);
        DockLayoutName = "Default";
        SelectedDockLayout = "Default";
        DockLayoutSummary = "Layout Default restaurado.";
        await RefreshDockLayoutsAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task RefreshThemesAsync(CancellationToken cancellationToken)
    {
        ThemeEngineState = ThemeEngineState.Loading;
        var themes = await _themeService.GetThemesAsync(cancellationToken);
        Themes.Clear();

        foreach (var theme in themes)
        {
            var option = new ThemeOptionViewModel(theme);
            Themes.Add(option);
            if (option.IsActive)
            {
                SelectedTheme = option;
            }
        }

        ApplyAvaloniaTheme(SelectedTheme?.Id ?? "System");
        ThemeEngineState = ThemeEngineState.Ready;
        ThemeSummary = SelectedTheme is null ? "Tema do sistema" : $"Tema {SelectedTheme.Name}";
    }

    [RelayCommand]
    private async Task ApplySelectedThemeAsync(CancellationToken cancellationToken)
    {
        if (SelectedTheme is null)
        {
            return;
        }

        ThemeEngineState = ThemeEngineState.Applying;
        var snapshot = await _themeService.ApplyThemeAsync(SelectedTheme.Id, cancellationToken);
        ApplyAvaloniaTheme(snapshot.ActiveTheme.Id);
        ThemeEngineState = snapshot.State;
        ThemeSummary = $"Tema {snapshot.ActiveTheme.Name}";
        await RefreshThemesAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task OpenCommandPaletteAsync(CancellationToken cancellationToken)
    {
        IsCommandPaletteOpen = true;
        CommandPaletteState = CommandPaletteState.Opening;
        CommandPaletteQuery = null;
        await LoadRecentCommandPaletteItemsAsync(cancellationToken);
        CommandPaletteState = CommandPaletteState.Searching;
    }

    [RelayCommand]
    private void CloseCommandPalette()
    {
        IsCommandPaletteOpen = false;
        CommandPaletteState = CommandPaletteState.Hidden;
        CommandPaletteItems.Clear();
        SelectedCommandPaletteItem = null;
    }

    [RelayCommand]
    private async Task ExecuteSelectedCommandPaletteItemAsync(CancellationToken cancellationToken)
    {
        if (SelectedCommandPaletteItem is null)
        {
            return;
        }

        CommandPaletteState = CommandPaletteState.Executing;
        try
        {
            await _commandPaletteService.ExecuteAsync(SelectedCommandPaletteItem.Id, cancellationToken);
            CommandPaletteState = CommandPaletteState.Completed;
            CloseCommandPalette();
        }
        catch
        {
            CommandPaletteState = CommandPaletteState.Failed;
            throw;
        }
    }

    [RelayCommand]
    private async Task ExecuteShellCommandAsync(string? commandId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commandId))
        {
            return;
        }

        await HandleShellCommandAsync(commandId, cancellationToken);
    }

    private async Task HandleShellCommandAsync(string commandId, CancellationToken cancellationToken)
    {
        switch (commandId)
        {
            case "navigation.dashboard":
                SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Dashboard);
                break;
            case "navigation.library":
                SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Library);
                break;
            case "navigation.devices":
                SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Devices);
                break;
            case "navigation.diagnostics":
                SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Diagnostics);
                break;
            case "navigation.settings":
                SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Settings);
                break;
            case "navigation.conversion":
                SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Conversion);
                break;
            case "library.refresh":
                await RefreshLibraryAsync(cancellationToken);
                break;
            case "devices.refresh":
                await RefreshDevicesAsync(cancellationToken);
                break;
            case "import.open":
                SelectedImportStep = ImportWizardStep.SelectingFiles;
                StatusMessage = "Fluxo de importação pronto.";
                break;
            case "conversion.open":
                SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Conversion);
                SelectedConversionStep = ConversionWizardStep.SelectingSource;
                StatusMessage = "Fluxo de conversão pronto.";
                break;
            case "theme.light":
                await ApplyThemeByIdAsync("Light", cancellationToken);
                break;
            case "theme.dark":
                await ApplyThemeByIdAsync("Dark", cancellationToken);
                break;
            case "dock.reset":
                await ResetDockLayoutAsync(cancellationToken);
                break;
            case "status.refresh":
                await RefreshStatusBarAsync(cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Command '{commandId}' is not supported by the shell.");
        }
    }

    [RelayCommand]
    private void ExitApplication()
    {
        if (AvaloniaApp.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    [RelayCommand]
    private async Task SelectAllVisibleGamesAsync(CancellationToken cancellationToken)
    {
        if (Games.Count == 0)
        {
            StatusMessage = "Nenhum jogo visível para selecionar.";
            return;
        }

        var snapshot = await _libraryViewService.SelectGamesAsync(Games.Select(game => game.Id).ToArray(), cancellationToken);
        ApplyLibrarySnapshot(snapshot, refreshGames: false);
        StatusMessage = $"{snapshot.SelectedGameIds.Count} jogo(s) selecionado(s).";
    }

    [RelayCommand]
    private void CopySelection()
    {
        StatusMessage = SelectedGame is null
            ? "Selecione um jogo para copiar."
            : $"Jogo {SelectedGame.Id} pronto para cópia.";
    }

    [RelayCommand]
    private void ShowPendingFeature(string? featureName)
    {
        var displayName = string.IsNullOrWhiteSpace(featureName) ? "Recurso" : featureName;
        StatusMessage = $"{displayName} ainda será conectado ao fluxo completo.";
    }

    [RelayCommand]
    private void ShowAbout()
    {
        StatusMessage = $"WiiStudio {StatusVersion} - ambiente de gerenciamento Wii.";
    }

    [RelayCommand]
    private async Task RunDiagnosticsAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Diagnostics);
        StatusMessage = "Executando diagnóstico...";

        try
        {
            var report = await _diagnosticsService.RunAsync(cancellationToken);
            var unhealthyCount = report.Checks.Count(check => check.Status == DiagnosticsStatus.Unhealthy);
            var warningCount = report.Checks.Count(check => check.Status == DiagnosticsStatus.Warning);
            StatusMessage = report.IsHealthy
                ? $"Diagnóstico concluído: {report.Checks.Count} verificação(ões), nenhum erro crítico."
                : $"Diagnóstico concluído: {unhealthyCount} erro(s), {warningCount} aviso(s).";
            await RefreshStatusBarAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SynchronizeSelectedMetadataAsync(CancellationToken cancellationToken)
    {
        var gameIds = GetSelectedGameIds();
        if (gameIds.Count == 0)
        {
            StatusMessage = "Selecione ao menos um jogo para sincronizar metadados.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Sincronizando metadados...";

        try
        {
            var result = await _metadataSynchronizationService.SynchronizeAsync(
                new MetadataSyncRequest(gameIds, ForceRefresh: true),
                cancellationToken);
            StatusMessage = result.HasFailures
                ? $"Metadados processados com pendências: {result.UpdatedCount} atualizado(s), {result.Items.Count} item(ns)."
                : $"Metadados sincronizados: {result.UpdatedCount} atualizado(s).";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DownloadSelectedCoversAsync(CancellationToken cancellationToken)
    {
        var gameIds = GetSelectedGameIds();
        if (gameIds.Count == 0)
        {
            StatusMessage = "Selecione ao menos um jogo para baixar capas.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Baixando capas...";

        try
        {
            var result = await _coverDownloadService.DownloadAsync(
                new CoverDownloadRequest(gameIds, ForceRefresh: false),
                cancellationToken);
            var downloadedCount = result.Items.Count(item => item.Source == CoverAssetSource.Provider);
            var cachedCount = result.Items.Count(item => item.Source == CoverAssetSource.Cache);
            var missingCount = result.Items.Count(item => item.Source == CoverAssetSource.Missing);
            StatusMessage = $"Capas processadas: {downloadedCount} baixada(s), {cachedCount} em cache, {missingCount} ausente(s).";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RemoveSelectedGamesAsync(CancellationToken cancellationToken)
    {
        var gameIds = GetSelectedGameIds();
        if (gameIds.Count == 0)
        {
            StatusMessage = "Selecione ao menos um jogo para remover.";
            return;
        }

        if (SelectedDevice is null || !Guid.TryParse(SelectedDevice.DeviceId, out var driveId))
        {
            StatusMessage = "Selecione o dispositivo de origem antes de remover jogos.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Removendo jogo(s) selecionado(s)...";

        try
        {
            foreach (var gameId in gameIds)
            {
                await _removeGameUseCase.ExecuteAsync(new RemoveGameCommand(gameId, driveId, Confirmed: true), cancellationToken);
            }

            StatusMessage = $"{gameIds.Count} jogo(s) removido(s).";
            await RefreshLibraryAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ManagePluginsAsync(CancellationToken cancellationToken)
    {
        SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Settings);
        var descriptors = await _pluginDiscoveryService.DiscoverAsync(cancellationToken);
        StatusMessage = descriptors.Count == 0
            ? "Nenhum plugin instalado encontrado."
            : $"{descriptors.Count} plugin(s) encontrado(s).";
    }

    [RelayCommand]
    private async Task ReloadPluginsAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        StatusMessage = "Recarregando plugins...";

        try
        {
            foreach (var runningPlugin in _pluginRegistry.RunningPlugins.ToArray())
            {
                await _pluginLoaderService.UnloadAsync(runningPlugin.Id, cancellationToken);
            }

            var descriptors = await _pluginDiscoveryService.DiscoverAsync(cancellationToken);
            var loadable = descriptors
                .Where(descriptor => descriptor.State == PluginDiscoveryState.ReadyForLoad)
                .ToArray();
            var results = new List<PluginLoadResult>();

            foreach (var descriptor in loadable)
            {
                results.Add(await _pluginLoaderService.LoadAsync(descriptor, cancellationToken));
            }

            var loadedCount = results.Count(result => result.Success);
            var failedCount = results.Count - loadedCount;
            StatusMessage = $"Plugins recarregados: {loadedCount} carregado(s), {failedCount} falha(s), {descriptors.Count} descoberto(s).";
        }
        finally
        {
            IsBusy = false;
        }
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

    partial void OnCommandPaletteQueryChanged(string? value)
    {
        if (!IsCommandPaletteOpen)
        {
            return;
        }

        _ = SearchCommandPaletteAsync(value, CancellationToken.None);
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
        _selectedGameIds = snapshot.SelectedGameIds;
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

    private async Task SearchCommandPaletteAsync(string? query, CancellationToken cancellationToken)
    {
        CommandPaletteState = CommandPaletteState.Searching;
        var results = await _commandPaletteService.SearchAsync(query ?? string.Empty, cancellationToken);
        ApplyCommandPaletteItems(results);
    }

    private async Task LoadRecentCommandPaletteItemsAsync(CancellationToken cancellationToken)
    {
        var recent = await _commandPaletteService.GetRecentCommandsAsync(cancellationToken);
        if (recent.Count == 0)
        {
            recent = await _commandPaletteService.SearchAsync(string.Empty, cancellationToken);
        }

        ApplyCommandPaletteItems(recent);
    }

    private void ApplyCommandPaletteItems(IReadOnlyList<CommandItem> commands)
    {
        CommandPaletteItems.Clear();
        foreach (var command in commands)
        {
            CommandPaletteItems.Add(new CommandPaletteItemViewModel(command));
        }

        SelectedCommandPaletteItem = CommandPaletteItems.FirstOrDefault();
    }

    private IReadOnlyList<string> GetSelectedGameIds()
    {
        if (_selectedGameIds.Count > 0)
        {
            return _selectedGameIds.ToArray();
        }

        return SelectedGame is null ? [] : [SelectedGame.Id];
    }

    private async Task ApplyThemeByIdAsync(string themeId, CancellationToken cancellationToken)
    {
        var snapshot = await _themeService.ApplyThemeAsync(themeId, cancellationToken);
        ApplyAvaloniaTheme(snapshot.ActiveTheme.Id);
        await RefreshThemesAsync(cancellationToken);
    }

    private static void ApplyAvaloniaTheme(string themeId)
    {
        if (AvaloniaApp.Current is null)
        {
            return;
        }

        AvaloniaApp.Current.RequestedThemeVariant = themeId switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
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
