using WiiStudio.Application.DTOs;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class UnknownPrivilegeStrategy : IOperatingSystemPrivilegeStrategy
{
    public OSPlatformKind Platform => OSPlatformKind.Unknown;

    public Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateStatus());
    }

    public Task<PrivilegedProcessResult> RunAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var status = CreateStatus();
        return Task.FromResult(new PrivilegedProcessResult(126, string.Empty, status.Message ?? string.Empty, TimeSpan.Zero, status));
    }

    private static PrivilegedAccessStatus CreateStatus()
    {
        return new PrivilegedAccessStatus(
            OSPlatformKind.Unknown,
            PrivilegedAccessState.Unsupported,
            CanRetry: false,
            RequiresUserAction: true,
            "Sistema operacional não suportado para acesso privilegiado a dispositivos.",
            "Use Linux, Windows ou macOS, ou configure a ferramenta manualmente fora do WiiStudio.",
            "No platform strategy matched the current OS.");
    }
}
