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
    private readonly IImportWizardService _importWizardService;
    private readonly ILibraryViewService _libraryViewService;
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

    public MainViewModel(
        DetectUsbDrivesUseCase detectUsbDrivesUseCase,
        IImportWizardService importWizardService,
        ILibraryViewService libraryViewService,
        IMainWindowService mainWindowService)
    {
        _detectUsbDrivesUseCase = detectUsbDrivesUseCase;
        _importWizardService = importWizardService;
        _libraryViewService = libraryViewService;
        _mainWindowService = mainWindowService;

        foreach (var module in _mainWindowService.Modules)
        {
            Modules.Add(module);
        }
    }

    public ObservableCollection<ShellModule> Modules { get; } = new();

    public ObservableCollection<UsbDrive> Drives { get; } = new();

    public ObservableCollection<LibraryGameItem> Games { get; } = new();

    public ObservableCollection<ImportPreviewItemViewModel> ImportPreviewItems { get; } = new();

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

        await RefreshLibraryAsync(cancellationToken);
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
        }
        finally
        {
            IsBusy = false;
        }
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
}
