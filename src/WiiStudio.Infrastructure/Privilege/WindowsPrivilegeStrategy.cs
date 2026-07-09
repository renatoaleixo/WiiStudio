using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class WindowsPrivilegeStrategy : IOperatingSystemPrivilegeStrategy
{
    private readonly IWindowsElevationDetector _elevationDetector;
    private readonly IProcessRunner _processRunner;

    public WindowsPrivilegeStrategy(IWindowsElevationDetector elevationDetector, IProcessRunner processRunner)
    {
        _elevationDetector = elevationDetector;
        _processRunner = processRunner;
    }

    public OSPlatformKind Platform => OSPlatformKind.Windows;

    public Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var isAdmin = _elevationDetector.IsAdministrator();
        return Task.FromResult(CreateStatus(
            isAdmin ? PrivilegedAccessState.Authorized : PrivilegedAccessState.AuthorizationRequired,
            canRetry: !isAdmin,
            requiresUserAction: !isAdmin,
            isAdmin
                ? "WiiStudio está em uma sessão administrativa no Windows."
                : "O Windows pode exigir que o WiiStudio seja executado como administrador para acessar discos WBFS brutos.",
            isAdmin
                ? "Nenhuma ação necessária."
                : "Feche o WiiStudio e abra como administrador antes de operar em discos físicos.",
            $"Tool={request.ToolId}; Device={request.DevicePath ?? "-"}; Administrator={isAdmin}."));
    }

    public async Task<PrivilegedProcessResult> RunAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken)
    {
        if (!_elevationDetector.IsAdministrator())
        {
            var authorizationStatus = CreateStatus(
                PrivilegedAccessState.AuthorizationRequired,
                canRetry: true,
                requiresUserAction: true,
                "A operação exige sessão administrativa no Windows.",
                "Reabra o WiiStudio como administrador e tente novamente.",
                "Windows non-elevated process cannot access the requested device.");
            return new PrivilegedProcessResult(5, string.Empty, authorizationStatus.Message ?? string.Empty, TimeSpan.Zero, authorizationStatus);
        }

        var result = await _processRunner.RunAsync(
            new ProcessRequest(request.ExecutablePath, request.Arguments, request.Timeout),
            cancellationToken);
        var status = CreateStatus(
            result.IsSuccess ? PrivilegedAccessState.Authorized : PrivilegedAccessState.Failed,
            canRetry: !result.IsSuccess,
            requiresUserAction: !result.IsSuccess,
            result.IsSuccess ? "Operação concluída com sessão administrativa." : "A ferramenta retornou erro mesmo em sessão administrativa.",
            result.IsSuccess ? "Nenhuma ação necessária." : "Verifique o dispositivo, permissões do Windows e saída da ferramenta.",
            result.StandardError);

        return new PrivilegedProcessResult(result.ExitCode, result.StandardOutput, result.StandardError, result.Duration, status);
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
            OSPlatformKind.Windows,
            state,
            canRetry,
            requiresUserAction,
            message,
            recommendedAction,
            diagnosticDetails);
    }
}
