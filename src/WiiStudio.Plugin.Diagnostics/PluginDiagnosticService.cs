using WiiStudio.Plugin.Abstractions;

namespace WiiStudio.Plugin.Diagnostics;

public interface IPluginDiagnosticService
{
    void Record(PluginDiagnosticEvent diagnosticEvent);
    IReadOnlyList<PluginDiagnosticEvent> GetEvents(string pluginId);
    PluginDiagnosticReport BuildReport(string pluginId, string pluginName, string pluginVersion, PluginLoadState state);
}

public sealed class PluginDiagnosticService : IPluginDiagnosticService
{
    private readonly List<PluginDiagnosticEvent> events = [];
    private readonly object syncRoot = new();

    public void Record(PluginDiagnosticEvent diagnosticEvent)
    {
        lock (syncRoot)
        {
            events.Add(diagnosticEvent);
        }
    }

    public IReadOnlyList<PluginDiagnosticEvent> GetEvents(string pluginId)
    {
        lock (syncRoot)
        {
            return events
                .Where(item => string.Equals(item.PluginId, pluginId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Timestamp)
                .ToArray();
        }
    }

    public PluginDiagnosticReport BuildReport(string pluginId, string pluginName, string pluginVersion, PluginLoadState state)
    {
        var pluginEvents = GetEvents(pluginId);
        var recommendations = pluginEvents
            .Where(item => item.Severity >= DiagnosticSeverity.Error)
            .Select(item => item.EventType switch
            {
                "manifest.invalid" => "Revise o plugin.json e valide os campos obrigatórios.",
                "permission.denied" => "Revise as permissões solicitadas pelo plugin antes de carregar.",
                "loading.failed" => "Confirme o assembly de entrada e o tipo principal do plugin.",
                _ => "Consulte os detalhes do diagnóstico do plugin."
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new PluginDiagnosticReport(pluginId, pluginName, pluginVersion, state, pluginEvents, recommendations);
    }
}
