using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class MacOSPrivilegeStrategy : IOperatingSystemPrivilegeStrategy
{
    private readonly IProcessRunner _processRunner;

    public MacOSPrivilegeStrategy(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public OSPlatformKind Platform => OSPlatformKind.MacOS;

    public Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateStatus(
            PrivilegedAccessState.AuthorizationRequired,
            canRetry: true,
            requiresUserAction: true,
            "macOS pode exigir permissão elevada para acessar discos físicos.",
            "Conceda permissões ao aplicativo ou execute em uma sessão autorizada pelo sistema.",
            $"Tool={request.ToolId}; Device={request.DevicePath ?? "-"}"));
    }

    public async Task<PrivilegedProcessResult> RunAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _processRunner.RunAsync(
            new ProcessRequest(request.ExecutablePath, request.Arguments, request.Timeout),
            cancellationToken);
        var status = CreateStatus(
            result.IsSuccess
                ? PrivilegedAccessState.Authorized
                : IsPermissionDenied(result)
                    ? PrivilegedAccessState.PermissionDenied
                    : PrivilegedAccessState.Failed,
            canRetry: !result.IsSuccess,
            requiresUserAction: !result.IsSuccess,
            result.IsSuccess ? "Operação concluída no macOS." : "macOS negou ou bloqueou a operação no dispositivo.",
            result.IsSuccess ? "Nenhuma ação necessária." : "Revise as permissões do app e do dispositivo no macOS.",
            result.StandardError);

        return new PrivilegedProcessResult(result.ExitCode, result.StandardOutput, result.StandardError, result.Duration, status);
    }

    private static bool IsPermissionDenied(ProcessResult result)
    {
        return result.StandardError.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)
            || result.StandardOutput.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("Operation not permitted", StringComparison.OrdinalIgnoreCase);
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
            OSPlatformKind.MacOS,
            state,
            canRetry,
            requiresUserAction,
            message,
            recommendedAction,
            diagnosticDetails);
    }
}
