using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class CommandPaletteService : ICommandPaletteService
{
    private const int RecentCommandLimit = 20;

    private static readonly CommandItem[] DefaultCommands =
    [
        new("navigation.dashboard", "Abrir Dashboard", CommandCategory.Navigation, "Ir para o painel inicial.", ["dashboard", "home", "inicio"]),
        new("navigation.library", "Abrir Biblioteca", CommandCategory.Navigation, "Ir para a biblioteca de jogos.", ["biblioteca", "jogos", "library"]),
        new("navigation.devices", "Abrir Device Manager", CommandCategory.Devices, "Ir para dispositivos conectados.", ["device", "dispositivos", "usb", "hd"]),
        new("navigation.diagnostics", "Abrir Diagnóstico", CommandCategory.Diagnostics, "Ir para verificações e diagnóstico.", ["diagnostico", "diagnostics", "check"]),
        new("library.refresh", "Atualizar Biblioteca", CommandCategory.Library, "Recarregar jogos e favoritos.", ["scan", "refresh", "atualizar jogos"]),
        new("devices.refresh", "Detectar Dispositivos", CommandCategory.Devices, "Procurar HDs, SSDs e pendrives.", ["usb", "detectar", "drives"]),
        new("import.open", "Importar Jogos", CommandCategory.Import, "Preparar fluxo de importação.", ["import", "iso", "wbfs", "rvz", "nkit"]),
        new("conversion.open", "Converter Jogos", CommandCategory.Conversion, "Preparar fluxo de conversão.", ["convert", "converter", "wbfs", "iso"]),
        new("theme.light", "Aplicar Tema Claro", CommandCategory.Settings, "Alternar para tema claro.", ["light", "claro", "tema"]),
        new("theme.dark", "Aplicar Tema Escuro", CommandCategory.Settings, "Alternar para tema escuro.", ["dark", "escuro", "tema"]),
        new("dock.reset", "Restaurar Layout", CommandCategory.Settings, "Resetar layout de painéis.", ["dock", "layout", "reset"]),
        new("status.refresh", "Atualizar Status", CommandCategory.Navigation, "Atualizar indicadores da barra de status.", ["status", "barra"])
    ];

    private readonly ICommandExecutor _commandExecutor;
    private readonly ICommandHistoryStore _historyStore;

    public CommandPaletteService(ICommandExecutor commandExecutor, ICommandHistoryStore historyStore)
    {
        _commandExecutor = commandExecutor;
        _historyStore = historyStore;
    }

    public Task<IReadOnlyList<CommandItem>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedQuery = Normalize(query);
        var results = DefaultCommands
            .Select(command => command with { Score = Score(command, normalizedQuery) })
            .Where(command => string.IsNullOrWhiteSpace(normalizedQuery) || command.Score > 0)
            .OrderByDescending(command => command.Score)
            .ThenBy(command => command.Title, StringComparer.CurrentCultureIgnoreCase)
            .Take(20)
            .ToArray();

        return Task.FromResult<IReadOnlyList<CommandItem>>(results);
    }

    public async Task ExecuteAsync(string commandId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        cancellationToken.ThrowIfCancellationRequested();

        if (!DefaultCommands.Any(command => command.Id.Equals(commandId, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Command '{commandId}' was not found.");
        }

        await _commandExecutor.ExecuteAsync(commandId, cancellationToken);
        await AddRecentCommandAsync(commandId, cancellationToken);
    }

    public async Task<IReadOnlyList<CommandItem>> GetRecentCommandsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var commandIds = await _historyStore.LoadAsync(cancellationToken);
        return commandIds
            .Select(commandId => DefaultCommands.FirstOrDefault(command => command.Id.Equals(commandId, StringComparison.OrdinalIgnoreCase)))
            .Where(command => command is not null)
            .Select(command => command!)
            .ToArray();
    }

    private async Task AddRecentCommandAsync(string commandId, CancellationToken cancellationToken)
    {
        var commandIds = await _historyStore.LoadAsync(cancellationToken);
        var updated = commandIds
            .Where(id => !id.Equals(commandId, StringComparison.OrdinalIgnoreCase))
            .Prepend(commandId)
            .Take(RecentCommandLimit)
            .ToArray();

        await _historyStore.SaveAsync(updated, cancellationToken);
    }

    private static int Score(CommandItem command, string normalizedQuery)
    {
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return 1;
        }

        var candidates = command.Aliases
            .Append(command.Title)
            .Append(command.Description)
            .Append(command.Category.ToString())
            .Select(Normalize);

        return candidates.Max(candidate => ScoreCandidate(candidate, normalizedQuery));
    }

    private static int ScoreCandidate(string candidate, string query)
    {
        if (candidate.Equals(query, StringComparison.Ordinal))
        {
            return 1000;
        }

        if (candidate.StartsWith(query, StringComparison.Ordinal))
        {
            return 750 - candidate.Length;
        }

        if (candidate.Contains(query, StringComparison.Ordinal))
        {
            return 500 - candidate.IndexOf(query, StringComparison.Ordinal);
        }

        return IsSubsequence(query, candidate) ? 250 - candidate.Length : 0;
    }

    private static bool IsSubsequence(string query, string candidate)
    {
        var queryIndex = 0;
        foreach (var character in candidate)
        {
            if (queryIndex < query.Length && character == query[queryIndex])
            {
                queryIndex++;
            }
        }

        return queryIndex == query.Length;
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }
}
