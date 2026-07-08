using WiiStudio.Plugin.Abstractions;

namespace WiiStudio.Plugin.Security;

public sealed class PluginPermissionService : IPluginPermissionService
{
    private readonly Dictionary<string, HashSet<PluginPermission>> grantedPermissions = new(StringComparer.OrdinalIgnoreCase);
    private readonly IPluginPermissionPolicy policy;

    public PluginPermissionService()
        : this(new DefaultPluginPermissionPolicy())
    {
    }

    public PluginPermissionService(IPluginPermissionPolicy policy)
    {
        this.policy = policy;
    }

    public void Grant(string pluginId, IEnumerable<PluginPermission> permissions)
    {
        if (!grantedPermissions.TryGetValue(pluginId, out var current))
        {
            current = [];
            grantedPermissions[pluginId] = current;
        }

        foreach (var permission in permissions)
        {
            current.Add(permission);
        }
    }

    public void Revoke(string pluginId, PluginPermission permission)
    {
        if (grantedPermissions.TryGetValue(pluginId, out var current))
        {
            current.Remove(permission);
        }
    }

    public bool HasPermission(string pluginId, PluginPermission permission) =>
        grantedPermissions.TryGetValue(pluginId, out var current) && current.Contains(permission);

    public Task<PermissionDecision> RequestPermissionAsync(
        string pluginId,
        PluginPermission permission,
        CancellationToken cancellationToken = default)
    {
        var decision = policy.Evaluate(permission);
        if (decision == PermissionDecision.Granted)
        {
            Grant(pluginId, [permission]);
        }

        return Task.FromResult(decision);
    }

    public Task<IReadOnlyList<PluginPermission>> GetGrantedPermissionsAsync(
        string pluginId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PluginPermission> result = grantedPermissions.TryGetValue(pluginId, out var current)
            ? current.Order().ToArray()
            : [];

        return Task.FromResult(result);
    }
}

public interface IPluginPermissionPolicy
{
    PermissionDecision Evaluate(PluginPermission permission);
    PluginRiskLevel GetRiskLevel(PluginPermission permission);
}

public sealed class DefaultPluginPermissionPolicy : IPluginPermissionPolicy
{
    private static readonly HashSet<PluginPermission> AutoGranted =
    [
        PluginPermission.AccessMetadata,
        PluginPermission.AccessAssets,
        PluginPermission.UiExtension
    ];

    public PermissionDecision Evaluate(PluginPermission permission) =>
        AutoGranted.Contains(permission)
            ? PermissionDecision.Granted
            : PermissionDecision.RequiresUserApproval;

    public PluginRiskLevel GetRiskLevel(PluginPermission permission) =>
        permission switch
        {
            PluginPermission.AccessMetadata or PluginPermission.AccessAssets or PluginPermission.UiExtension => PluginRiskLevel.Low,
            PluginPermission.NetworkAccess or PluginPermission.AccessGameLibrary or PluginPermission.AccessDiagnostics => PluginRiskLevel.Medium,
            PluginPermission.FileSystemWrite or PluginPermission.ModifyGameLibrary or PluginPermission.ModifyMetadata or PluginPermission.ModifyAssets => PluginRiskLevel.High,
            PluginPermission.ExecuteProcess or PluginPermission.BackgroundTasks => PluginRiskLevel.Critical,
            _ => PluginRiskLevel.Unknown
        };
}
