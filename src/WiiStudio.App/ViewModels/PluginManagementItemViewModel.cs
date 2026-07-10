using WiiStudio.Plugin.Abstractions;
using WiiStudio.Plugin.Hosting;

namespace WiiStudio.App.ViewModels;

public sealed class PluginManagementItemViewModel
{
    public PluginManagementItemViewModel(PluginManagementItem item)
    {
        Id = item.Id;
        Name = item.Name;
        Version = item.Version;
        Description = string.IsNullOrWhiteSpace(item.Description) ? "Sem descrição." : item.Description;
        Author = string.IsNullOrWhiteSpace(item.Author) ? "Autor não informado" : item.Author;
        DirectoryPath = item.DirectoryPath;
        State = item.State;
        LoadState = item.LoadState;
        Capabilities = string.Join(", ", item.Capabilities.Select(ToDisplayText));
        Permissions = item.Permissions.Select(ToDisplayText).ToArray();
        Warnings = item.Warnings.ToArray();
        Errors = item.Errors.ToArray();
        Diagnostics = item.Diagnostics
            .OrderByDescending(diagnostic => diagnostic.Timestamp)
            .Select(diagnostic => $"{diagnostic.Timestamp:HH:mm:ss} · {diagnostic.Severity} · {diagnostic.MessageKey}")
            .ToArray();
    }

    public string Id { get; }
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public string Author { get; }
    public string DirectoryPath { get; }
    public PluginDiscoveryState State { get; }
    public PluginLoadState LoadState { get; }
    public string Capabilities { get; }
    public IReadOnlyList<string> Permissions { get; }
    public IReadOnlyList<string> Warnings { get; }
    public IReadOnlyList<string> Errors { get; }
    public IReadOnlyList<string> Diagnostics { get; }

    public string StateText => State switch
    {
        PluginDiscoveryState.ReadyForLoad => "Pronto",
        PluginDiscoveryState.Disabled => "Desabilitado",
        PluginDiscoveryState.Quarantined => "Quarentena",
        PluginDiscoveryState.InvalidManifest => "Manifest inválido",
        PluginDiscoveryState.Duplicate => "Duplicado",
        PluginDiscoveryState.Incompatible => "Incompatível",
        _ => State.ToString()
    };

    public string Summary => $"{Version} · {StateText} · {Author}";
    public bool IsReady => State == PluginDiscoveryState.ReadyForLoad;
    public bool IsDisabled => State == PluginDiscoveryState.Disabled;
    public bool HasIssues => Errors.Count > 0 || State is PluginDiscoveryState.InvalidManifest or PluginDiscoveryState.Duplicate or PluginDiscoveryState.Quarantined;
    public bool CanEnable => IsDisabled;
    public bool CanDisable => State != PluginDiscoveryState.Disabled && State != PluginDiscoveryState.Quarantined;
    public bool CanRemove => State != PluginDiscoveryState.Unknown;

    private static string ToDisplayText(PluginCapability capability) =>
        capability switch
        {
            PluginCapability.MetadataProvider => "Metadados",
            PluginCapability.AssetProvider => "Assets",
            PluginCapability.ConversionProvider => "Conversão",
            PluginCapability.DiagnosticsProvider => "Diagnóstico",
            PluginCapability.UiExtension => "Interface",
            PluginCapability.Automation => "Automação",
            _ => capability.ToString()
        };

    private static string ToDisplayText(PluginPermissionStatus permission) =>
        $"{ToDisplayText(permission.Permission)} · {ToDisplayText(permission.RiskLevel)} · {(permission.IsGranted ? "Liberada" : "Pendente")}";

    private static string ToDisplayText(PluginPermission permission) =>
        permission switch
        {
            PluginPermission.FileSystemRead => "Ler arquivos",
            PluginPermission.FileSystemWrite => "Escrever arquivos",
            PluginPermission.NetworkAccess => "Rede",
            PluginPermission.ExecuteProcess => "Executar processo",
            PluginPermission.AccessGameLibrary => "Ler biblioteca",
            PluginPermission.ModifyGameLibrary => "Alterar biblioteca",
            PluginPermission.AccessMetadata => "Ler metadados",
            PluginPermission.ModifyMetadata => "Alterar metadados",
            PluginPermission.AccessAssets => "Ler assets",
            PluginPermission.ModifyAssets => "Alterar assets",
            PluginPermission.AccessDiagnostics => "Ler diagnóstico",
            PluginPermission.UiExtension => "Extensão visual",
            PluginPermission.BackgroundTasks => "Tarefas em segundo plano",
            _ => permission.ToString()
        };

    private static string ToDisplayText(PluginRiskLevel risk) =>
        risk switch
        {
            PluginRiskLevel.Low => "baixo risco",
            PluginRiskLevel.Medium => "risco médio",
            PluginRiskLevel.High => "alto risco",
            PluginRiskLevel.Critical => "risco crítico",
            _ => "risco desconhecido"
        };
}
