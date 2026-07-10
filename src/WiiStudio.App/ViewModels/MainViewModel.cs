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
    private readonly IBatchCoverDownloadService _batchCoverDownloadService;
    private readonly ICoverAssetBulkActionService _coverAssetBulkActionService;
    private readonly DetectUsbDrivesUseCase _detectUsbDrivesUseCase;
    private readonly IConversionWizardService _conversionWizardService;
    private readonly ICoverCache _coverCache;
    private readonly ICoverDownloadService _coverDownloadService;
    private readonly IDashboardService _dashboardService;
    private readonly IDeviceManagerService _deviceManagerService;
    private readonly IDiagnosticsService _diagnosticsService;
    private readonly IDockService _dockService;
    private readonly IExportGameService _exportGameService;
    private readonly IImportWizardService _importWizardService;
    private readonly IKeyboardShortcutService _keyboardShortcutService;
    private readonly IGameLibraryService _gameLibraryService;
    private readonly ILibraryViewService _libraryViewService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IMetadataCacheService _metadataCacheService;
    private readonly IMetadataSynchronizationService _metadataSynchronizationService;
    private readonly IPluginDiscoveryService _pluginDiscoveryService;
    private readonly IPluginLoaderService _pluginLoaderService;
    private readonly IPluginRegistry _pluginRegistry;
    private readonly RemoveGameUseCase _removeGameUseCase;
    private readonly ISettingsService _settingsService;
    private readonly IStatusBarService _statusBarService;
    private readonly IThemeService _themeService;
    private Guid? _activeLibraryDriveId;
    private CancellationTokenSource? _coverBatchCancellation;
    private bool _isApplyingDeviceSnapshot;
    private bool _isApplyingLibraryViewMode;
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
    private LibraryViewModeOptionViewModel? _selectedLibraryViewModeOption;

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

    [ObservableProperty]
    private string _diagnosticsSummary = "Diagnóstico não executado.";

    [ObservableProperty]
    private string _coverBatchStatus = "Nenhum download em andamento.";

    [ObservableProperty]
    private bool _isCoverBatchRunning;

    [ObservableProperty]
    private bool _isCoverLightboxOpen;

    [ObservableProperty]
    private CoverAssetViewModel? _selectedCoverAsset;

    [ObservableProperty]
    private LibraryGameItem? _selectedAssetGame;

    public MainViewModel(
        ICommandPaletteService commandPaletteService,
        IBatchCoverDownloadService batchCoverDownloadService,
        ICoverAssetBulkActionService coverAssetBulkActionService,
        DetectUsbDrivesUseCase detectUsbDrivesUseCase,
        IConversionWizardService conversionWizardService,
        ICoverCache coverCache,
        ICoverDownloadService coverDownloadService,
        IDashboardService dashboardService,
        IDeviceManagerService deviceManagerService,
        IDiagnosticsService diagnosticsService,
        IDockService dockService,
        IExportGameService exportGameService,
        IImportWizardService importWizardService,
        IKeyboardShortcutService keyboardShortcutService,
        IGameLibraryService gameLibraryService,
        ILibraryViewService libraryViewService,
        IMainWindowService mainWindowService,
        IMetadataCacheService metadataCacheService,
        IMetadataSynchronizationService metadataSynchronizationService,
        IPluginDiscoveryService pluginDiscoveryService,
        IPluginLoaderService pluginLoaderService,
        IPluginRegistry pluginRegistry,
        RemoveGameUseCase removeGameUseCase,
        ISettingsService settingsService,
        IStatusBarService statusBarService,
        ShellCommandExecutor shellCommandExecutor,
        IThemeService themeService)
    {
        _commandPaletteService = commandPaletteService;
        _batchCoverDownloadService = batchCoverDownloadService;
        _coverAssetBulkActionService = coverAssetBulkActionService;
        _detectUsbDrivesUseCase = detectUsbDrivesUseCase;
        _conversionWizardService = conversionWizardService;
        _coverCache = coverCache;
        _coverDownloadService = coverDownloadService;
        _dashboardService = dashboardService;
        _deviceManagerService = deviceManagerService;
        _diagnosticsService = diagnosticsService;
        _dockService = dockService;
        _exportGameService = exportGameService;
        _importWizardService = importWizardService;
        _keyboardShortcutService = keyboardShortcutService;
        _gameLibraryService = gameLibraryService;
        _libraryViewService = libraryViewService;
        _mainWindowService = mainWindowService;
        _metadataCacheService = metadataCacheService;
        _metadataSynchronizationService = metadataSynchronizationService;
        _pluginDiscoveryService = pluginDiscoveryService;
        _pluginLoaderService = pluginLoaderService;
        _pluginRegistry = pluginRegistry;
        _removeGameUseCase = removeGameUseCase;
        _settingsService = settingsService;
        _statusBarService = statusBarService;
        _themeService = themeService;
        shellCommandExecutor.SetHandler(HandleShellCommandAsync);

        foreach (var module in _mainWindowService.Modules)
        {
            Modules.Add(module);
        }

        SelectLibraryViewModeOption(SelectedLibraryViewMode);
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

    public ObservableCollection<CoverAssetViewModel> CoverAssets { get; } = new();

    public ObservableCollection<LibraryGameItem> AssetSelectedGames { get; } = new();

    public ObservableCollection<ImportPreviewItemViewModel> ImportPreviewItems { get; } = new();

    public ObservableCollection<ConversionPreviewItemViewModel> ConversionPreviewItems { get; } = new();

    public ObservableCollection<DiagnosticsCheckItemViewModel> DiagnosticsChecks { get; } = new();

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

    public IReadOnlyList<LibraryViewModeOptionViewModel> LibraryViewModes { get; } =
    [
        new(LibraryViewMode.Grid, "Grade"),
        new(LibraryViewMode.List, "Lista"),
        new(LibraryViewMode.Compact, "Compacta"),
        new(LibraryViewMode.Detailed, "Detalhada")
    ];

    public string SelectedModuleTitle => SelectedModule?.Title ?? "Dashboard";

    public string SelectedModuleDescription => SelectedModule?.Description ?? "Resumo do ambiente e atividades recentes.";

    public bool IsDashboardModuleSelected => IsSelectedModule(ShellModuleId.Dashboard);

    public bool IsLibraryModuleSelected => IsSelectedModule(ShellModuleId.Library);

    public bool IsDevicesModuleSelected => IsSelectedModule(ShellModuleId.Devices);

    public bool IsConversionModuleSelected => IsSelectedModule(ShellModuleId.Conversion);

    public bool IsAssetsModuleSelected => IsSelectedModule(ShellModuleId.Assets);

    public bool IsTasksModuleSelected => IsSelectedModule(ShellModuleId.Tasks);

    public bool IsNotificationsModuleSelected => IsSelectedModule(ShellModuleId.Notifications);

    public bool IsSettingsModuleSelected => IsSelectedModule(ShellModuleId.Settings);

    public bool IsDiagnosticsModuleSelected => IsSelectedModule(ShellModuleId.Diagnostics);

    public bool IsLogsModuleSelected => IsSelectedModule(ShellModuleId.Logs);

    public string LibrarySummary => LibraryTotalCount == 1 ? "1 jogo" : $"{LibraryTotalCount} jogos";

    public bool IsLibraryGridViewSelected => SelectedLibraryViewMode == LibraryViewMode.Grid;

    public bool IsLibraryListViewSelected => SelectedLibraryViewMode == LibraryViewMode.List;

    public bool IsLibraryCompactViewSelected => SelectedLibraryViewMode == LibraryViewMode.Compact;

    public bool IsLibraryDetailedViewSelected => SelectedLibraryViewMode == LibraryViewMode.Detailed;

    public bool IsLibraryTableViewSelected => SelectedLibraryViewMode != LibraryViewMode.Grid;

    public int DashboardGamesWithCovers => Games.Count(game => game.HasCover);

    public int DashboardGamesWithMetadata => Games.Count(game => game.HasMetadata);

    public int DashboardFavoriteGames => Games.Count(game => game.IsFavorite);

    public double DashboardCoverProgress => CalculateDashboardProgress(DashboardGamesWithCovers);

    public double DashboardMetadataProgress => CalculateDashboardProgress(DashboardGamesWithMetadata);

    public double DashboardFavoriteProgress => CalculateDashboardProgress(DashboardFavoriteGames);

    public string DashboardCoverSummary => FormatDashboardCoverage(DashboardGamesWithCovers, "com capa");

    public string DashboardMetadataSummary => FormatDashboardCoverage(DashboardGamesWithMetadata, "com metadados");

    public string DashboardFavoriteSummary => FormatDashboardCoverage(DashboardFavoriteGames, "favorito(s)");

    public string DashboardLibraryHealthSummary => LibraryTotalCount == 0
        ? "Carregue um HD ou atualize a biblioteca para montar o panorama."
        : $"{DashboardGamesWithCovers}/{LibraryTotalCount} capas, {DashboardGamesWithMetadata}/{LibraryTotalCount} metadados";

    public string SelectionSummary => SelectedGameCount == 0
        ? "Nenhum jogo selecionado"
        : SelectedGameCount == 1
            ? "1 jogo selecionado"
            : $"{SelectedGameCount} jogos selecionados";

    public string AssetSelectionSummary => SelectedGameCount == 0
        ? "Selecione jogos na Biblioteca para acompanhar capas, assets e metadados."
        : $"{SelectionSummary}: {AssetSelectedGames.Count(game => game.HasCover)} com capa, {AssetSelectedGames.Count(game => game.HasMetadata)} com metadados.";

    public string AssetGallerySummary => AssetPreviewGame is null
        ? "Nenhum jogo selecionado para pré-visualização."
        : $"{AssetPreviewGame.Title} - {CoverAssets.Count(asset => asset.HasImage)}/{CoverAssets.Count} asset(s) em cache.";

    public string AssetMetadataSummary => AssetPreviewGame is null
        ? "Selecione um jogo para ver os metadados sincronizados."
        : AssetPreviewGame.HasMetadata
            ? $"{AssetPreviewGame.MetadataSummary} - {AssetPreviewGame.Genres}"
            : "Metadados ainda não sincronizados para este jogo.";

    private LibraryGameItem? AssetPreviewGame => SelectedAssetGame ?? SelectedGame;

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

    public async Task SetSelectedLibraryItemsAsync(IReadOnlyList<LibraryGameItem> selectedItems, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(selectedItems);

        var selectedIds = selectedItems
            .Select(item => item.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var snapshot = await _libraryViewService.SelectGamesAsync(selectedIds, cancellationToken);
        await ApplyLibrarySnapshotAsync(snapshot, cancellationToken, refreshGames: false);
        SelectedGame = selectedItems.LastOrDefault();
        await RefreshSelectedAssetGalleryAsync(cancellationToken);
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
                new SearchRequest(UsbDriveId: _activeLibraryDriveId, SearchText: LibrarySearchText),
                cancellationToken);

            await ApplyLibrarySnapshotAsync(snapshot, cancellationToken);
            StatusMessage = snapshot.TotalCount == 0
                ? "Biblioteca vazia."
                : $"{snapshot.TotalCount} jogo(s) carregado(s).";
            await RefreshStatusBarAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            LibraryState = LibraryViewState.Error;
            LibraryTotalCount = 0;
            Games.Clear();
            StatusMessage = $"Falha ao carregar biblioteca: {exception.Message}";
            OnPropertyChanged(nameof(LibrarySummary));
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
        await ApplyLibrarySnapshotAsync(snapshot, cancellationToken, refreshGames: false);
        SelectedGame = Games.LastOrDefault();
        await RefreshSelectedAssetGalleryAsync(cancellationToken);
        StatusMessage = $"{snapshot.SelectedGameIds.Count} jogo(s) selecionado(s).";
    }

    [RelayCommand]
    private async Task ClearGameSelectionAsync(CancellationToken cancellationToken)
    {
        var snapshot = await _libraryViewService.SelectGamesAsync([], cancellationToken);
        await ApplyLibrarySnapshotAsync(snapshot, cancellationToken, refreshGames: false);
        SelectedGame = null;
        CoverAssets.Clear();
        SelectedCoverAsset = null;
        RefreshAssetSelectedGames();
        StatusMessage = "Seleção limpa.";
    }

    [RelayCommand]
    private async Task ToggleGameSelectionAsync(LibraryGameItem? game, CancellationToken cancellationToken)
    {
        if (game is null)
        {
            return;
        }

        var selectedIds = Games
            .Where(item => item.IsSelected)
            .Select(item => item.Id)
            .ToArray();
        var snapshot = await _libraryViewService.SelectGamesAsync(selectedIds, cancellationToken);
        await ApplyLibrarySnapshotAsync(snapshot, cancellationToken, refreshGames: false);
        SelectedGame = game;
        await RefreshSelectedAssetGalleryAsync(cancellationToken);
    }

    [RelayCommand]
    private void CopySelection()
    {
        StatusMessage = SelectedGame is null
            ? "Selecione um jogo para copiar."
            : $"Jogo {SelectedGame.Id} pronto para cópia.";
    }

    [RelayCommand]
    private void ShowAbout()
    {
        StatusMessage = $"WiiStudio {StatusVersion} - ambiente de gerenciamento Wii.";
    }

    [RelayCommand]
    private async Task ExportSelectedGamesAsync(CancellationToken cancellationToken)
    {
        var gameIds = GetSelectedGameIds();
        if (gameIds.Count == 0)
        {
            StatusMessage = "Selecione ao menos um jogo para exportar.";
            return;
        }

        var settings = await _settingsService.GetAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.ExportPath))
        {
            SelectedModule = Modules.FirstOrDefault(module => module.Id == ShellModuleId.Settings);
            StatusMessage = "Configure uma pasta de exportação antes de exportar jogos.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Exportando jogo(s) selecionado(s)...";

        try
        {
            var result = await _exportGameService.ExportAsync(
                new ExportRequest(gameIds, settings.ExportPath, FileFormat.Wbfs, ExportConflictResolution.Rename),
                cancellationToken);
            var exportedCount = result.Items.Count(item => item.Status == ExportStatus.Exported);
            StatusMessage = result.HasFailures
                ? $"Exportação concluída com falhas: {exportedCount}/{result.TotalGames} exportado(s)."
                : $"{exportedCount} jogo(s) exportado(s) para {settings.ExportPath}.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowDocumentation()
    {
        StatusMessage = "Documentação local: docs/99-final e docs/08-tools.";
    }

    [RelayCommand]
    private void ShowKeyboardShortcuts()
    {
        var shortcuts = _keyboardShortcutService.GetActiveShortcuts(ShortcutScope.Global);
        StatusMessage = shortcuts.Count == 0
            ? "Nenhum atalho global registrado."
            : string.Join(" | ", shortcuts.Select(shortcut => $"{shortcut.Gesture}: {shortcut.CommandId}"));
    }

    public async Task SetImportSourcePathsAsync(IReadOnlyList<string> sourcePaths, CancellationToken cancellationToken)
    {
        ImportSourcePathsText = string.Join(Environment.NewLine, sourcePaths);
        await AnalyzeImportAsync(cancellationToken);
    }

    public async Task SetConversionSourcePathsAsync(IReadOnlyList<string> sourcePaths, CancellationToken cancellationToken)
    {
        ConversionSourcePathsText = string.Join(Environment.NewLine, sourcePaths);
        await AnalyzeConversionAsync(cancellationToken);
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
            ApplyDiagnosticsReport(report);
            var unhealthyCount = report.Checks.Count(check => check.Status == DiagnosticsStatus.Unhealthy);
            var warningCount = report.Checks.Count(check => check.Status == DiagnosticsStatus.Warning);
            DiagnosticsSummary = report.IsHealthy
                ? $"Diagnóstico concluído: {report.Checks.Count} verificação(ões), nenhum erro crítico."
                : $"Diagnóstico concluído: {unhealthyCount} erro(s), {warningCount} aviso(s).";
            StatusMessage = DiagnosticsSummary;
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
                new MetadataSyncRequest(gameIds, ForceRefresh: false),
                cancellationToken);
            var cachedCount = result.Items.Count(item => item.Status == MetadataSyncStatus.Cached);
            var missingCount = result.Items.Count(item => item.Status == MetadataSyncStatus.Missing);
            var unavailableCount = result.Items.Count(item => item.Status == MetadataSyncStatus.ProviderUnavailable);
            var failedCount = result.Items.Count(item => item.Status == MetadataSyncStatus.Failed);

            var metadataStatus = unavailableCount > 0
                ? $"Provider de metadados indisponível: {cachedCount} em cache, {missingCount} ausente(s), {failedCount} falha(s)."
                : $"Metadados processados: {result.UpdatedCount} atualizado(s), {cachedCount} em cache, {missingCount} ausente(s), {failedCount} falha(s).";
            await RefreshLibraryAsync(cancellationToken);
            StatusMessage = metadataStatus;
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
        IsCoverBatchRunning = true;
        CoverBatchStatus = $"Baixando capas 0/{gameIds.Count}";
        StatusMessage = "Baixando capas...";
        _coverBatchCancellation?.Dispose();
        _coverBatchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var progress = new Progress<CoverBatchProgress>(item =>
            {
                var current = string.IsNullOrWhiteSpace(item.CurrentGameId) ? "-" : item.CurrentGameId;
                CoverBatchStatus = $"Baixando capas {item.ProcessedGames}/{item.TotalGames} - atual: {current}";
                StatusMessage = CoverBatchStatus;
            });
            var summary = await _batchCoverDownloadService.DownloadAsync(
                gameIds,
                [],
                forceRefresh: false,
                progress,
                _coverBatchCancellation.Token);
            await RefreshLibraryAsync(cancellationToken);
            await RefreshSelectedAssetGalleryAsync(cancellationToken);
            CoverBatchStatus = summary.Cancelled
                ? $"Download cancelado: {summary.ProcessedGames}/{summary.Requested} jogo(s), {summary.Downloaded} baixada(s)."
                : $"Download concluído: {summary.Downloaded} baixada(s), {summary.FromCache} em cache, {summary.Missing} ausente(s), {summary.Failed} falha(s).";
            StatusMessage = CoverBatchStatus;
        }
        finally
        {
            IsCoverBatchRunning = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task ForceRefreshSelectedAssetsAsync(CancellationToken cancellationToken)
    {
        return ExecuteCoverAssetBulkActionAsync(
            CoverAssetBulkActionMode.ForceRefresh,
            forceRefresh: true,
            clearBeforeDownload: false,
            "Forçando atualização de assets...",
            cancellationToken);
    }

    [RelayCommand]
    private Task ClearSelectedAssetCacheAsync(CancellationToken cancellationToken)
    {
        return ExecuteCoverAssetBulkActionAsync(
            CoverAssetBulkActionMode.ClearCache,
            forceRefresh: false,
            clearBeforeDownload: false,
            "Limpando cache de assets...",
            cancellationToken);
    }

    [RelayCommand]
    private Task ValidateSelectedAssetsAsync(CancellationToken cancellationToken)
    {
        return ExecuteCoverAssetBulkActionAsync(
            CoverAssetBulkActionMode.ValidateCache,
            forceRefresh: false,
            clearBeforeDownload: false,
            "Validando assets em cache...",
            cancellationToken);
    }

    private async Task ExecuteCoverAssetBulkActionAsync(
        CoverAssetBulkActionMode mode,
        bool forceRefresh,
        bool clearBeforeDownload,
        string startMessage,
        CancellationToken cancellationToken)
    {
        var gameIds = GetSelectedGameIds();
        if (gameIds.Count == 0)
        {
            StatusMessage = "Selecione ao menos um jogo para operar assets.";
            return;
        }

        IsBusy = true;
        IsCoverBatchRunning = true;
        CoverBatchStatus = startMessage;
        StatusMessage = startMessage;
        _coverBatchCancellation?.Dispose();
        _coverBatchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var progress = new Progress<CoverAssetBulkActionProgress>(item =>
            {
                var current = string.IsNullOrWhiteSpace(item.CurrentGameId) ? "-" : item.CurrentGameId;
                CoverBatchStatus = $"Assets {item.ProcessedGames}/{item.TotalGames} - atual: {current}";
                StatusMessage = CoverBatchStatus;
            });
            var result = await _coverAssetBulkActionService.ExecuteAsync(
                new CoverAssetBulkActionRequest(
                    gameIds,
                    [],
                    "PT",
                    forceRefresh,
                    clearBeforeDownload,
                    mode),
                progress,
                _coverBatchCancellation.Token);
            await RefreshLibraryAsync(cancellationToken);
            await RefreshSelectedAssetGalleryAsync(cancellationToken);
            CoverBatchStatus = result.Cancelled
                ? $"Operação cancelada: {result.Downloaded} baixado(s), {result.Cleared} removido(s)."
                : $"Operação concluída: {result.Downloaded} baixado(s), {result.FromCache} em cache, {result.Cleared} removido(s), {result.Invalid} inválido(s), {result.Failed} falha(s).";
            StatusMessage = CoverBatchStatus;
        }
        finally
        {
            IsCoverBatchRunning = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelBatchOperation()
    {
        _coverBatchCancellation?.Cancel();
        CoverBatchStatus = "Cancelamento solicitado.";
        StatusMessage = CoverBatchStatus;
    }

    [RelayCommand]
    private void OpenCoverLightbox(CoverAssetViewModel? asset)
    {
        if (asset is not { HasImage: true })
        {
            StatusMessage = "Asset indisponível para ampliação.";
            return;
        }

        SelectedCoverAsset = asset;
        IsCoverLightboxOpen = true;
    }

    [RelayCommand]
    private void CloseCoverLightbox()
    {
        IsCoverLightboxOpen = false;
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
            var drives = await RefreshDriveListAsync(cancellationToken);
            SelectedImportDrive = Drives.FirstOrDefault(IsImportDriveReady) ?? Drives.FirstOrDefault();
            await LoadDriveLibraryAsync(SelectedImportDrive, cancellationToken);

            if (drives.Count == 0)
            {
                StatusMessage = "Nenhum dispositivo encontrado.";
            }

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
            await RefreshDriveListAsync(cancellationToken);
            SelectImportDriveForSelectedDevice();
            await LoadDriveLibraryAsync(SelectedImportDrive, cancellationToken);
            if (snapshot.Devices.Count == 0)
            {
                StatusMessage = "Nenhum dispositivo encontrado.";
            }

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
        CanExecuteImport = preview.CanImport && IsImportDriveReady(SelectedImportDrive) && HasEnoughSpaceForImport(sourcePaths, SelectedImportDrive);
        ImportSummary = CreateImportSummary(preview, sourcePaths, SelectedImportDrive);
        StatusMessage = CanExecuteImport
            ? "Arquivos e HD USB prontos para importação."
            : ImportSummary;
    }

    [RelayCommand]
    private async Task ExecuteImportAsync(CancellationToken cancellationToken)
    {
        if (!CanExecuteImport || SelectedImportDrive is null)
        {
            StatusMessage = "Selecione arquivos válidos e um HD USB montado/escrevível.";
            return;
        }

        IsBusy = true;
        ImportWizardState = ImportWizardState.Importing;
        SelectedImportStep = ImportWizardStep.Importing;
        StatusMessage = "Importando jogos...";

        try
        {
            var result = await _importWizardService.ExecuteAsync(
                new ImportWizardRequest(ParseImportSourcePaths(), SelectedImportDrive.Id, UserConfirmed: true),
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
        OnPropertyChanged(nameof(IsLibraryGridViewSelected));
        OnPropertyChanged(nameof(IsLibraryListViewSelected));
        OnPropertyChanged(nameof(IsLibraryCompactViewSelected));
        OnPropertyChanged(nameof(IsLibraryDetailedViewSelected));
        OnPropertyChanged(nameof(IsLibraryTableViewSelected));
        SelectLibraryViewModeOption(value);
        if (_isApplyingLibraryViewMode)
        {
            return;
        }

        _ = ChangeLibraryViewModeAsync(value);
    }

    partial void OnSelectedLibraryViewModeOptionChanged(LibraryViewModeOptionViewModel? value)
    {
        if (value is null || value.Mode == SelectedLibraryViewMode)
        {
            return;
        }

        SelectedLibraryViewMode = value.Mode;
    }

    partial void OnSelectedGameChanged(LibraryGameItem? value)
    {
        if (value is not null && _selectedGameIds.Contains(value.Id))
        {
            SelectedAssetGame = value;
            return;
        }

        if (SelectedAssetGame is null)
        {
            _ = RefreshSelectedAssetGalleryAsync(CancellationToken.None);
        }
    }

    partial void OnSelectedAssetGameChanged(LibraryGameItem? value)
    {
        if (value is not null && !ReferenceEquals(SelectedGame, value))
        {
            SelectedGame = value;
        }

        _ = RefreshSelectedAssetGalleryAsync(CancellationToken.None);
    }

    partial void OnSelectedImportDriveChanged(UsbDrive? value)
    {
        CanExecuteImport = value is not null
            && ImportPreviewItems.Count > 0
            && ImportPreviewItems.All(item => item.Status != ImportWizardFileStatus.Error.ToString())
            && IsImportDriveReady(value)
            && HasEnoughSpaceForImport(ParseImportSourcePaths(), value);
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

        if (_isApplyingDeviceSnapshot)
        {
            return;
        }

        SelectImportDriveForSelectedDevice();
        _ = LoadDriveLibraryAsync(SelectedImportDrive, CancellationToken.None);
    }

    partial void OnSelectedModuleChanged(ShellModule? value)
    {
        OnPropertyChanged(nameof(SelectedModuleTitle));
        OnPropertyChanged(nameof(SelectedModuleDescription));
        OnPropertyChanged(nameof(IsDashboardModuleSelected));
        OnPropertyChanged(nameof(IsLibraryModuleSelected));
        OnPropertyChanged(nameof(IsDevicesModuleSelected));
        OnPropertyChanged(nameof(IsConversionModuleSelected));
        OnPropertyChanged(nameof(IsAssetsModuleSelected));
        OnPropertyChanged(nameof(IsTasksModuleSelected));
        OnPropertyChanged(nameof(IsNotificationsModuleSelected));
        OnPropertyChanged(nameof(IsSettingsModuleSelected));
        OnPropertyChanged(nameof(IsDiagnosticsModuleSelected));
        OnPropertyChanged(nameof(IsLogsModuleSelected));

        if (value is null)
        {
            return;
        }

        _ = ActivateSelectedModuleAsync(value.Id);
    }

    private async Task ActivateSelectedModuleAsync(ShellModuleId moduleId)
    {
        try
        {
            await PersistSelectedModuleAsync(moduleId);
            switch (moduleId)
            {
                case ShellModuleId.Dashboard:
                    await RefreshDashboardAsync(CancellationToken.None);
                    break;
                case ShellModuleId.Library:
                    await RefreshLibraryAsync(CancellationToken.None);
                    break;
                case ShellModuleId.Devices:
                    await RefreshDevicesAsync(CancellationToken.None);
                    break;
                case ShellModuleId.Diagnostics:
                    await RunDiagnosticsAsync(CancellationToken.None);
                    break;
                case ShellModuleId.Settings:
                    await RefreshDockLayoutsAsync(CancellationToken.None);
                    await RefreshThemesAsync(CancellationToken.None);
                    break;
                case ShellModuleId.Assets:
                    StatusMessage = "Selecione jogos na biblioteca para baixar capas ou sincronizar metadados.";
                    break;
                case ShellModuleId.Tasks:
                    await RefreshStatusBarAsync(CancellationToken.None);
                    StatusMessage = TaskSummary;
                    break;
                case ShellModuleId.Notifications:
                    await RefreshStatusBarAsync(CancellationToken.None);
                    StatusMessage = NotificationSummary;
                    break;
                case ShellModuleId.Logs:
                    StatusMessage = "Logs recentes aparecem na barra de status e no console da aplicação.";
                    break;
                case ShellModuleId.Conversion:
                    StatusMessage = "Fluxo de conversão pronto.";
                    break;
            }
        }
        catch (Exception exception)
        {
            StatusMessage = $"Falha ao atualizar módulo: {exception.Message}";
        }
    }

    private async Task PersistSelectedModuleAsync(ShellModuleId moduleId)
    {
        await _mainWindowService.SelectModuleAsync(moduleId, CancellationToken.None);
        await _mainWindowService.SaveLayoutAsync(CancellationToken.None);
    }

    private bool IsSelectedModule(ShellModuleId moduleId)
    {
        return SelectedModule?.Id == moduleId;
    }

    private async Task ChangeLibraryViewModeAsync(LibraryViewMode mode)
    {
        var snapshot = await _libraryViewService.ChangeViewModeAsync(mode, CancellationToken.None);
        await ApplyLibrarySnapshotAsync(snapshot, CancellationToken.None, refreshGames: false);
    }

    private async Task SelectVisibleGamesAsync(LibraryGameItem? game)
    {
        IReadOnlyList<string> selectedIds = game is null ? [] : [game.Id];
        var snapshot = await _libraryViewService.SelectGamesAsync(selectedIds, CancellationToken.None);
        await ApplyLibrarySnapshotAsync(snapshot, CancellationToken.None, refreshGames: false);
        await RefreshSelectedAssetGalleryAsync(CancellationToken.None);
    }

    private async Task ApplyLibrarySnapshotAsync(
        LibraryViewSnapshot snapshot,
        CancellationToken cancellationToken,
        bool refreshGames = true)
    {
        LibraryState = snapshot.State;
        LibraryTotalCount = snapshot.TotalCount;
        _isApplyingLibraryViewMode = true;
        try
        {
            SelectedLibraryViewMode = snapshot.ViewMode;
            SelectLibraryViewModeOption(snapshot.ViewMode);
        }
        finally
        {
            _isApplyingLibraryViewMode = false;
        }
        _selectedGameIds = snapshot.SelectedGameIds;
        SelectedGameCount = snapshot.SelectedGameIds.Count;

        if (refreshGames)
        {
            Games.Clear();
            foreach (var game in snapshot.Games)
            {
                var coverPath = await _coverCache.GetAsync(game.Id, CoverAssetKind.FrontCover, cancellationToken);
                var metadata = await _metadataCacheService.GetAsync(game.Id, cancellationToken);
                Games.Add(new LibraryGameItem(game, coverPath, metadata));
            }
        }

        ApplySelectionState();
        RefreshAssetSelectedGames();
        OnPropertyChanged(nameof(LibrarySummary));
        OnPropertyChanged(nameof(SelectionSummary));
        OnDashboardIndicatorsChanged();
    }

    private void SelectLibraryViewModeOption(LibraryViewMode mode)
    {
        var option = LibraryViewModes.FirstOrDefault(item => item.Mode == mode);
        if (!ReferenceEquals(SelectedLibraryViewModeOption, option))
        {
            SelectedLibraryViewModeOption = option;
        }
    }

    private async Task RefreshSelectedAssetGalleryAsync(CancellationToken cancellationToken)
    {
        CoverAssets.Clear();
        SelectedCoverAsset = null;
        var previewGame = AssetPreviewGame;
        if (previewGame is null)
        {
            OnAssetPanelChanged();
            return;
        }

        foreach (var (kind, title) in GetGalleryAssets())
        {
            var path = await _coverCache.GetAsync(previewGame.Id, kind, cancellationToken);
            var asset = new CoverAssetViewModel(kind, title, path);
            CoverAssets.Add(asset);
            SelectedCoverAsset ??= asset.HasImage ? asset : null;
        }

        OnAssetPanelChanged();
    }

    private void ApplySelectionState()
    {
        foreach (var game in Games)
        {
            game.IsSelected = _selectedGameIds.Contains(game.Id);
        }
    }

    private void RefreshAssetSelectedGames()
    {
        var previousAssetGameId = SelectedAssetGame?.Id;
        AssetSelectedGames.Clear();
        foreach (var game in Games.Where(game => _selectedGameIds.Contains(game.Id)))
        {
            AssetSelectedGames.Add(game);
        }

        SelectedAssetGame = AssetSelectedGames.FirstOrDefault(game => string.Equals(game.Id, previousAssetGameId, StringComparison.OrdinalIgnoreCase))
            ?? AssetSelectedGames.FirstOrDefault(game => string.Equals(game.Id, SelectedGame?.Id, StringComparison.OrdinalIgnoreCase))
            ?? AssetSelectedGames.FirstOrDefault();

        _ = RefreshSelectedAssetGalleryAsync(CancellationToken.None);
        OnAssetPanelChanged();
    }

    private void OnAssetPanelChanged()
    {
        OnPropertyChanged(nameof(AssetSelectionSummary));
        OnPropertyChanged(nameof(AssetGallerySummary));
        OnPropertyChanged(nameof(AssetMetadataSummary));
    }

    private static IReadOnlyList<(CoverAssetKind Kind, string Title)> GetGalleryAssets()
    {
        return
        [
            (CoverAssetKind.FrontCover, "Frente"),
            (CoverAssetKind.BackCover, "Verso"),
            (CoverAssetKind.DiscArt, "Disco"),
            (CoverAssetKind.Banner, "Banner"),
            (CoverAssetKind.Icon, "Icon")
        ];
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
        OnDashboardIndicatorsChanged();
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
        OnPropertyChanged(nameof(DeviceSummary));
    }

    private void OnDashboardIndicatorsChanged()
    {
        OnPropertyChanged(nameof(DashboardGamesWithCovers));
        OnPropertyChanged(nameof(DashboardGamesWithMetadata));
        OnPropertyChanged(nameof(DashboardFavoriteGames));
        OnPropertyChanged(nameof(DashboardCoverProgress));
        OnPropertyChanged(nameof(DashboardMetadataProgress));
        OnPropertyChanged(nameof(DashboardFavoriteProgress));
        OnPropertyChanged(nameof(DashboardCoverSummary));
        OnPropertyChanged(nameof(DashboardMetadataSummary));
        OnPropertyChanged(nameof(DashboardFavoriteSummary));
        OnPropertyChanged(nameof(DashboardLibraryHealthSummary));
    }

    private double CalculateDashboardProgress(int count)
    {
        return LibraryTotalCount == 0 ? 0 : count * 100.0 / LibraryTotalCount;
    }

    private string FormatDashboardCoverage(int count, string label)
    {
        return LibraryTotalCount == 0 ? $"0 {label}" : $"{count}/{LibraryTotalCount} {label}";
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

    private void ApplyDiagnosticsReport(DiagnosticsReport report)
    {
        DiagnosticsChecks.Clear();
        foreach (var check in report.Checks)
        {
            DiagnosticsChecks.Add(new DiagnosticsCheckItemViewModel(check));
        }
    }

    private IReadOnlyList<string> GetSelectedGameIds()
    {
        if (_selectedGameIds.Count > 0)
        {
            return _selectedGameIds.ToArray();
        }

        return SelectedGame is null ? [] : [SelectedGame.Id];
    }

    private static bool IsImportDriveReady(UsbDrive? drive)
    {
        return drive is { IsMounted: true, IsWritable: true }
            && (drive.FileSystem is UsbDriveFileSystem.Wbfs
                or UsbDriveFileSystem.Fat32
                or UsbDriveFileSystem.Ntfs
                or UsbDriveFileSystem.ExFat);
    }

    private static bool HasEnoughSpaceForImport(IReadOnlyList<string> sourcePaths, UsbDrive? drive)
    {
        if (drive is null || sourcePaths.Count == 0)
        {
            return false;
        }

        var requiredBytes = sourcePaths
            .Where(File.Exists)
            .Select(path => new FileInfo(path).Length)
            .Sum();

        return requiredBytes <= drive.FreeSpace;
    }

    private static string CreateImportSummary(ImportPreview preview, IReadOnlyList<string> sourcePaths, UsbDrive? drive)
    {
        if (preview.Items.Count == 0)
        {
            return "Informe ao menos um arquivo.";
        }

        if (!preview.CanImport)
        {
            return "Revise os arquivos informados.";
        }

        if (drive is null)
        {
            return "Selecione o HD USB de destino.";
        }

        if (!IsImportDriveReady(drive))
        {
            return "O HD USB precisa estar montado, escrevível e em um formato compatível.";
        }

        if (!HasEnoughSpaceForImport(sourcePaths, drive))
        {
            return "Espaço livre insuficiente no HD USB selecionado.";
        }

        return $"{preview.Items.Count} arquivo(s) prontos para importar em {drive.Name}.";
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

        _isApplyingDeviceSnapshot = true;
        try
        {
            SelectedDevice = Devices.FirstOrDefault();
        }
        finally
        {
            _isApplyingDeviceSnapshot = false;
        }

        DeviceManagerSummary = Devices.Count == 0
            ? "Nenhum dispositivo"
            : $"{Devices.Count} dispositivo(s)";
    }

    private async Task<IReadOnlyList<UsbDrive>> RefreshDriveListAsync(CancellationToken cancellationToken)
    {
        var selectedDriveId = SelectedImportDrive?.Id;
        var drives = await _detectUsbDrivesUseCase.ExecuteAsync(cancellationToken);
        Drives.Clear();

        foreach (var drive in drives)
        {
            Drives.Add(drive);
        }

        SelectedImportDrive = selectedDriveId is null
            ? Drives.FirstOrDefault(IsImportDriveReady) ?? Drives.FirstOrDefault()
            : Drives.FirstOrDefault(drive => drive.Id == selectedDriveId.Value)
                ?? Drives.FirstOrDefault(IsImportDriveReady)
                ?? Drives.FirstOrDefault();

        OnPropertyChanged(nameof(DeviceSummary));
        return drives;
    }

    private void SelectImportDriveForSelectedDevice()
    {
        if (SelectedDevice is null || !Guid.TryParse(SelectedDevice.DeviceId, out var deviceId))
        {
            return;
        }

        SelectedImportDrive = Drives.FirstOrDefault(drive => drive.Id == deviceId) ?? SelectedImportDrive;
    }

    private async Task LoadDriveLibraryAsync(UsbDrive? drive, CancellationToken cancellationToken)
    {
        if (drive is null)
        {
            _activeLibraryDriveId = null;
            await RefreshLibraryAsync(cancellationToken);
            return;
        }

        _activeLibraryDriveId = drive.Id;
        LibraryState = LibraryViewState.Refreshing;
        StatusMessage = $"Lendo jogos de {drive.Name}...";

        try
        {
            await _gameLibraryService.RefreshAsync(drive.Id, cancellationToken);
            await RefreshLibraryAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            LibraryState = LibraryViewState.Error;
            LibraryTotalCount = 0;
            Games.Clear();
            StatusMessage = CreateLibraryLoadFailureMessage(drive, exception);
            OnPropertyChanged(nameof(LibrarySummary));
        }
    }

    private static string CreateLibraryLoadFailureMessage(UsbDrive drive, Exception exception)
    {
        if (drive.MountPoint.StartsWith("/dev/", StringComparison.OrdinalIgnoreCase))
        {
            return $"HD WBFS detectado em {drive.MountPoint}, mas o WiiStudio precisa de permissão para ler os jogos. Autorize o prompt do sistema ou execute com acesso ao dispositivo.";
        }

        return $"Falha ao ler jogos de {drive.Name}: {exception.Message}";
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
