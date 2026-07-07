using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Shell;

public sealed class JsonDockService : IDockService
{
    private const string DefaultLayoutName = "Default";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly DockPanelInfo[] DefaultPanels =
    [
        new("dashboard", "Dashboard", DockRegion.Workspace, DockPanelState.Docked, 0),
        new("library", "Biblioteca", DockRegion.Workspace, DockPanelState.Docked, 1),
        new("devices", "Dispositivos", DockRegion.Right, DockPanelState.Docked, 0),
        new("tasks", "Tarefas", DockRegion.Right, DockPanelState.Docked, 1),
        new("notifications", "Notificações", DockRegion.Right, DockPanelState.Docked, 2),
        new("logs", "Logs", DockRegion.Bottom, DockPanelState.Docked, 0),
        new("diagnostics", "Diagnóstico", DockRegion.Bottom, DockPanelState.Hidden, 1)
    ];

    private readonly DockLayoutStoreOptions _options;

    public JsonDockService(DockLayoutStoreOptions options)
    {
        _options = options;
        CurrentLayout = CreateDefaultLayout();
    }

    public DockLayout CurrentLayout { get; private set; }

    public async Task SaveLayoutAsync(string layoutName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedName = NormalizeLayoutName(layoutName);
        Directory.CreateDirectory(_options.LayoutDirectoryPath);

        CurrentLayout = CurrentLayout with
        {
            Name = normalizedName,
            State = DockLayoutState.SavingLayout,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await using var stream = File.Create(GetLayoutPath(normalizedName));
        await JsonSerializer.SerializeAsync(stream, CurrentLayout with { State = DockLayoutState.Ready }, JsonOptions, cancellationToken);
        CurrentLayout = CurrentLayout with { State = DockLayoutState.Ready };
    }

    public async Task LoadLayoutAsync(string layoutName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedName = NormalizeLayoutName(layoutName);
        var path = GetLayoutPath(normalizedName);

        if (!File.Exists(path))
        {
            if (normalizedName == DefaultLayoutName)
            {
                await ResetLayoutAsync(cancellationToken);
                return;
            }

            throw new FileNotFoundException($"Dock layout '{normalizedName}' was not found.", path);
        }

        await using var stream = File.OpenRead(path);
        var layout = await JsonSerializer.DeserializeAsync<DockLayout>(stream, JsonOptions, cancellationToken);
        CurrentLayout = Normalize(layout ?? CreateDefaultLayout(), normalizedName) with { State = DockLayoutState.Ready };
    }

    public async Task ResetLayoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentLayout = CreateDefaultLayout() with { State = DockLayoutState.RestoringLayout };
        await SaveLayoutAsync(DefaultLayoutName, cancellationToken);
        CurrentLayout = CreateDefaultLayout();
    }

    public Task<IReadOnlyList<string>> GetLayoutsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(_options.LayoutDirectoryPath))
        {
            return Task.FromResult<IReadOnlyList<string>>([DefaultLayoutName]);
        }

        var layouts = Directory
            .EnumerateFiles(_options.LayoutDirectoryPath, "*.layout.json")
            .Select(path => Path.GetFileName(path).Replace(".layout.json", string.Empty, StringComparison.OrdinalIgnoreCase))
            .Append(DefaultLayoutName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(layouts);
    }

    private string GetLayoutPath(string layoutName)
    {
        return Path.Combine(_options.LayoutDirectoryPath, $"{layoutName}.layout.json");
    }

    private static DockLayout CreateDefaultLayout()
    {
        return new DockLayout(DefaultLayoutName, DefaultPanels, DockLayoutState.Ready, DateTimeOffset.UtcNow);
    }

    private static DockLayout Normalize(DockLayout layout, string fallbackName)
    {
        var panels = layout.Panels
            .Where(panel => !string.IsNullOrWhiteSpace(panel.Id) && !string.IsNullOrWhiteSpace(panel.Title))
            .Select(panel => panel with
            {
                Width = Math.Clamp(panel.Width, 160, 900),
                Height = Math.Clamp(panel.Height, 120, 700)
            })
            .ToArray();

        return layout with
        {
            Name = string.IsNullOrWhiteSpace(layout.Name) ? fallbackName : NormalizeLayoutName(layout.Name),
            Panels = panels.Length == 0 ? DefaultPanels : panels
        };
    }

    private static string NormalizeLayoutName(string layoutName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutName);
        var invalidChars = Path.GetInvalidFileNameChars();
        var normalized = new string(layoutName.Trim().Select(character => invalidChars.Contains(character) ? '-' : character).ToArray());

        return string.IsNullOrWhiteSpace(normalized) ? DefaultLayoutName : normalized;
    }
}
