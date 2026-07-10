using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Shell;

public sealed class JsonMainWindowService : IMainWindowService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly ShellModule[] DefaultModules =
    [
        new(ShellModuleId.Dashboard, "Dashboard", "Resumo do ambiente e atividades recentes."),
        new(ShellModuleId.Library, "Biblioteca", "Jogos encontrados nos dispositivos e caches locais."),
        new(ShellModuleId.Devices, "Dispositivos", "HDs USB, partições e capacidade disponível."),
        new(ShellModuleId.Conversion, "Conversão", "Fila de conversões, importações e exportações."),
        new(ShellModuleId.Assets, "Capas e Assets", "Capas, banners, disc art e metadados visuais."),
        new(ShellModuleId.Tasks, "Tarefas", "Operações em andamento e histórico."),
        new(ShellModuleId.Notifications, "Notificações", "Avisos, erros e eventos recentes."),
        new(ShellModuleId.Plugins, "Plugins", "Instalação local, permissões e diagnóstico de plugins."),
        new(ShellModuleId.Settings, "Configurações", "Preferências, perfis e caminhos."),
        new(ShellModuleId.Diagnostics, "Diagnóstico", "Verificações do ambiente e dependências."),
        new(ShellModuleId.Logs, "Logs", "Eventos estruturados e exportação para suporte.")
    ];

    private readonly MainWindowStoreOptions _options;

    public JsonMainWindowService(MainWindowStoreOptions options)
    {
        _options = options;
        CurrentLayout = new MainWindowLayout();
    }

    public MainWindowLayout CurrentLayout { get; private set; }

    public IReadOnlyList<ShellModule> Modules => DefaultModules;

    public async Task<MainWindowSnapshot> InitializeAsync(CancellationToken cancellationToken)
    {
        CurrentLayout = await RestoreLayoutAsync(cancellationToken);

        return new MainWindowSnapshot(MainWindowState.Ready, CurrentLayout, Modules, DateTimeOffset.UtcNow);
    }

    public async Task<MainWindowLayout> RestoreLayoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.LayoutFilePath))
        {
            return new MainWindowLayout();
        }

        await using var stream = File.OpenRead(_options.LayoutFilePath);
        var layout = await JsonSerializer.DeserializeAsync<MainWindowLayout>(stream, JsonOptions, cancellationToken);

        return Normalize(layout ?? new MainWindowLayout());
    }

    public async Task SaveLayoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(_options.LayoutFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_options.LayoutFilePath);
        await JsonSerializer.SerializeAsync(stream, Normalize(CurrentLayout), JsonOptions, cancellationToken);
    }

    public Task SelectModuleAsync(ShellModuleId moduleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Modules.Any(module => module.Id == moduleId && module.IsAvailable))
        {
            throw new InvalidOperationException($"Module '{moduleId}' is not available.");
        }

        CurrentLayout = CurrentLayout with { ActiveModuleId = moduleId };

        return Task.CompletedTask;
    }

    public Task SetSidePanelVisibilityAsync(bool isVisible, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentLayout = CurrentLayout with { IsSidePanelVisible = isVisible };

        return Task.CompletedTask;
    }

    private static MainWindowLayout Normalize(MainWindowLayout layout)
    {
        var activeModuleId = Enum.IsDefined(layout.ActiveModuleId)
            ? layout.ActiveModuleId
            : ShellModuleId.Dashboard;

        return layout with
        {
            ActiveModuleId = activeModuleId,
            NavigationWidth = Math.Clamp(layout.NavigationWidth, 180, 360),
            SidePanelWidth = Math.Clamp(layout.SidePanelWidth, 240, 480)
        };
    }
}
