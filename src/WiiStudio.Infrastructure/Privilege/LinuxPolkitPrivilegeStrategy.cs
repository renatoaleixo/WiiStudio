using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class LinuxPolkitPrivilegeStrategy : IOperatingSystemPrivilegeStrategy
{
    private readonly IProcessRunner _processRunner;
    private readonly IPolkitPolicyInstaller _policyInstaller;

    public LinuxPolkitPrivilegeStrategy(IProcessRunner processRunner, IPolkitPolicyInstaller policyInstaller)
    {
        _processRunner = processRunner;
        _policyInstaller = policyInstaller;
    }

    public OSPlatformKind Platform => OSPlatformKind.Linux;

    public async Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var policyStatus = await _policyInstaller.CheckAsync(cancellationToken);
        if (policyStatus.State == PolkitPolicyInstallState.PkexecMissing)
        {
            return CreateStatus(
                PrivilegedAccessState.HelperMissing,
                canRetry: false,
                requiresUserAction: true,
                policyStatus.MessageKey ?? "pkexec não está disponível para solicitar permissão administrativa.",
                policyStatus.RecommendedActionKey ?? "Instale o pacote polkit/pkexec do sistema e execute o diagnóstico novamente.",
                policyStatus.DiagnosticDetails ?? "Linux privileged access requires pkexec.");
        }

        if (policyStatus.State == PolkitPolicyInstallState.Missing)
        {
            return CreateStatus(
                PrivilegedAccessState.PolicyMissing,
                canRetry: true,
                requiresUserAction: true,
                policyStatus.MessageKey ?? "A política Polkit do WiiStudio não está instalada.",
                policyStatus.RecommendedActionKey ?? "Instale a policy oficial do WiiStudio.",
                policyStatus.DiagnosticDetails ?? $"Missing policy: {policyStatus.PolicyPath}.");
        }

        if (policyStatus.State is PolkitPolicyInstallState.Invalid or PolkitPolicyInstallState.ValidationFailed)
        {
            return CreateStatus(
                PrivilegedAccessState.Failed,
                canRetry: true,
                requiresUserAction: true,
                policyStatus.MessageKey ?? "A política Polkit do WiiStudio é inválida.",
                policyStatus.RecommendedActionKey ?? "Reinstale a policy oficial do WiiStudio.",
                policyStatus.DiagnosticDetails ?? "Policy validation failed.");
        }

        return CreateStatus(
            PrivilegedAccessState.AuthorizationRequired,
            canRetry: true,
            requiresUserAction: true,
            "O Linux pode solicitar a senha administrativa para acessar partições WBFS brutas.",
            "Autorize o prompt do sistema; a sessão Polkit pode manter a liberação por alguns minutos.",
            $"Policy detected. Tool={request.ToolId}; Device={request.DevicePath ?? "-"}");
    }

    public async Task<PrivilegedProcessResult> RunAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken)
    {
        var pkexecPath = FindExecutable(PolkitPolicyConstants.PkexecExecutable);
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
