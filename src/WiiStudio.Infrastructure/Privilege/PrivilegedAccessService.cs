using Microsoft.Extensions.Logging;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class PrivilegedAccessService : IPrivilegedAccessService
{
    private static readonly HashSet<string> AllowedToolIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "wit",
        "wwt"
    };

    private readonly IOperatingSystemInfo _operatingSystemInfo;
    private readonly IReadOnlyDictionary<OSPlatformKind, IOperatingSystemPrivilegeStrategy> _strategies;
    private readonly ILogger<PrivilegedAccessService> _logger;

    public PrivilegedAccessService(
        IOperatingSystemInfo operatingSystemInfo,
        IEnumerable<IOperatingSystemPrivilegeStrategy> strategies,
        ILogger<PrivilegedAccessService> logger)
    {
        _operatingSystemInfo = operatingSystemInfo;
        _strategies = strategies.ToDictionary(strategy => strategy.Platform);
        _logger = logger;
    }

    public async Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = ValidateTool(request.ToolId, request.ExecutablePath);
        if (validation is not null)
        {
            return validation;
        }

        return await GetStrategy().CheckStatusAsync(request, cancellationToken);
    }

    public async Task<PrivilegedProcessResult> RunKnownToolAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = ValidateRequest(request);
        if (validation is not null)
        {
            _logger.LogWarning("Blocked privileged request for tool {ToolId}: {Reason}", request.ToolId, validation.Message);
            return new PrivilegedProcessResult(126, string.Empty, validation.Message ?? "Privileged request blocked.", TimeSpan.Zero, validation);
        }

        _logger.LogInformation(
            "Running privileged operation {Operation} for known tool {ToolId} on {Platform}",
            request.Operation,
            request.ToolId,
            _operatingSystemInfo.Current);
        return await GetStrategy().RunAsync(request, cancellationToken);
    }

    private IOperatingSystemPrivilegeStrategy GetStrategy()
    {
        return _strategies.TryGetValue(_operatingSystemInfo.Current, out var strategy)
            ? strategy
            : _strategies[OSPlatformKind.Unknown];
    }

    private static PrivilegedAccessStatus? ValidateRequest(PrivilegedToolRequest request)
    {
        return ValidateTool(request.ToolId, request.ExecutablePath)
            ?? ValidateArguments(request.Arguments)
            ?? ValidateOperation(request);
    }

    private static PrivilegedAccessStatus? ValidateTool(string toolId, string executablePath)
    {
        if (!AllowedToolIds.Contains(toolId))
        {
            return Failed($"Tool '{toolId}' is not allowed for privileged execution.");
        }

        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
        {
            return Failed("Privileged tool executable was not found.");
        }

        if (executablePath.Contains("..", StringComparison.Ordinal))
        {
            return Failed("Privileged tool executable path is not normalized.");
        }

        var fileName = Path.GetFileName(executablePath);
        if (!string.Equals(fileName, toolId, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Path.GetFileNameWithoutExtension(fileName), toolId, StringComparison.OrdinalIgnoreCase))
        {
            return Failed("Privileged tool executable does not match the requested tool id.");
        }

        return null;
    }

    private static PrivilegedAccessStatus? ValidateArguments(IReadOnlyList<string> arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument.Contains('\0')
                || argument.Contains("&&", StringComparison.Ordinal)
                || argument.Contains('|')
                || argument.Contains(';')
                || argument.Contains('>'))
            {
                return Failed("Privileged tool argument contains unsafe shell syntax.");
            }
        }

        return null;
    }

    private static PrivilegedAccessStatus? ValidateOperation(PrivilegedToolRequest request)
    {
        if (!string.Equals(request.ToolId, "wwt", StringComparison.OrdinalIgnoreCase))
        {
            return request.Operation is PrivilegedDeviceOperation.InspectDevice or PrivilegedDeviceOperation.ReadWbfsMetadata
                ? null
                : Failed("Only inspect/read operations are allowed for privileged wit execution.");
        }

        var expectedCommand = request.Operation switch
        {
            PrivilegedDeviceOperation.InspectDevice => "find",
            PrivilegedDeviceOperation.ListWbfsGames => "list",
            PrivilegedDeviceOperation.AddWbfsGame => "add",
            PrivilegedDeviceOperation.RemoveWbfsGame => "remove",
            _ => null
        };

        if (expectedCommand is null || request.Arguments.Count == 0 || !string.Equals(request.Arguments[0], expectedCommand, StringComparison.OrdinalIgnoreCase))
        {
            return Failed("Privileged wwt operation does not match the allowed command catalog.");
        }

        if (!string.IsNullOrWhiteSpace(request.DevicePath)
            && !request.Arguments.Any(argument => string.Equals(argument, request.DevicePath, StringComparison.OrdinalIgnoreCase)))
        {
            return Failed("Privileged request device path is not present in structured arguments.");
        }

        return null;
    }

    private static PrivilegedAccessStatus Failed(string message)
    {
        return new PrivilegedAccessStatus(
            OSPlatformKind.Unknown,
            PrivilegedAccessState.Failed,
            CanRetry: false,
            RequiresUserAction: false,
            message,
            "Revise ferramenta, caminho e argumentos permitidos.",
            message);
    }
}
