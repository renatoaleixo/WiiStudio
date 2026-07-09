using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class LinuxPolkitPrivilegeStrategy : IOperatingSystemPrivilegeStrategy
{
    private const string PkexecExecutable = "pkexec";
    private const string WiiStudioPolicyPath = "/usr/share/polkit-1/actions/com.wiistudio.device.policy";
    private const string LegacyWwtPolicyPath = "/usr/share/polkit-1/actions/com.wiistudio.wwt.policy";

    private readonly IProcessRunner _processRunner;

    public LinuxPolkitPrivilegeStrategy(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public OSPlatformKind Platform => OSPlatformKind.Linux;

    public Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (FindExecutable(PkexecExecutable) is null)
        {
            return Task.FromResult(CreateStatus(
                PrivilegedAccessState.HelperMissing,
                canRetry: false,
                requiresUserAction: true,
                "pkexec não está disponível para solicitar permissão administrativa.",
                "Instale o pacote polkit/pkexec do sistema e execute o diagnóstico novamente.",
                "Linux privileged access requires pkexec."));
        }

        if (!File.Exists(WiiStudioPolicyPath) && !File.Exists(LegacyWwtPolicyPath))
        {
            return Task.FromResult(CreateStatus(
                PrivilegedAccessState.PolicyMissing,
                canRetry: true,
                requiresUserAction: true,
                "A política Polkit do WiiStudio não está instalada.",
                $"Instale uma política com auth_admin_keep em {WiiStudioPolicyPath}.",
                $"Missing policy: {WiiStudioPolicyPath}. Legacy policy checked: {LegacyWwtPolicyPath}."));
        }

        return Task.FromResult(CreateStatus(
            PrivilegedAccessState.AuthorizationRequired,
            canRetry: true,
            requiresUserAction: true,
            "O Linux pode solicitar a senha administrativa para acessar partições WBFS brutas.",
            "Autorize o prompt do sistema; a sessão Polkit pode manter a liberação por alguns minutos.",
            $"Policy detected. Tool={request.ToolId}; Device={request.DevicePath ?? "-"}"));
    }

    public async Task<PrivilegedProcessResult> RunAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken)
    {
        var pkexecPath = FindExecutable(PkexecExecutable);
        if (pkexecPath is null)
        {
            var missingHelperStatus = CreateStatus(
                PrivilegedAccessState.HelperMissing,
                canRetry: false,
                requiresUserAction: true,
                "pkexec não está disponível para executar a operação privilegiada.",
                "Instale o polkit/pkexec ou execute o WiiStudio com permissões compatíveis.",
                "pkexec not found in PATH.");
            return new PrivilegedProcessResult(127, string.Empty, missingHelperStatus.Message ?? string.Empty, TimeSpan.Zero, missingHelperStatus);
        }

        var startedAt = DateTimeOffset.UtcNow;
        var result = await _processRunner.RunAsync(
            new ProcessRequest(pkexecPath, [request.ExecutablePath, .. request.Arguments], request.Timeout),
            cancellationToken);
        var status = CreateStatus(
            result.IsSuccess
                ? PrivilegedAccessState.Authorized
                : IsPermissionDenied(result)
                    ? PrivilegedAccessState.PermissionDenied
                    : PrivilegedAccessState.Failed,
            canRetry: !result.IsSuccess,
            requiresUserAction: !result.IsSuccess,
            result.IsSuccess
                ? "Operação privilegiada concluída."
                : "A operação privilegiada não foi autorizada ou falhou.",
            result.IsSuccess
                ? "Nenhuma ação necessária."
                : "Autorize o prompt do sistema e tente novamente; se persistir, rode o diagnóstico.",
            result.StandardError);

        return new PrivilegedProcessResult(
            result.ExitCode,
            result.StandardOutput,
            result.StandardError,
            result.Duration == TimeSpan.Zero ? DateTimeOffset.UtcNow - startedAt : result.Duration,
            status);
    }

    private static bool IsPermissionDenied(ProcessResult result)
    {
        return result.StandardError.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("authorization failed", StringComparison.OrdinalIgnoreCase)
            || result.StandardOutput.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)
            || result.StandardOutput.Contains("authorization failed", StringComparison.OrdinalIgnoreCase);
    }

    private static PrivilegedAccessStatus CreateStatus(
        PrivilegedAccessState state,
        bool canRetry,
        bool requiresUserAction,
        string message,
        string recommendedAction,
        string diagnosticDetails)
    {
        return new PrivilegedAccessStatus(
            OSPlatformKind.Linux,
            state,
            canRetry,
            requiresUserAction,
            message,
            recommendedAction,
            diagnosticDetails);
    }

    private static string? FindExecutable(string executableName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory, executableName))
            .FirstOrDefault(File.Exists);
    }
}
