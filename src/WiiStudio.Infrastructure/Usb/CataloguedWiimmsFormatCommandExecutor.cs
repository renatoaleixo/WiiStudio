using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Usb;

public sealed class CataloguedWiimmsFormatCommandExecutor : IWiimmsFormatCommandExecutor
{
    private static readonly TimeSpan FormatTimeout = TimeSpan.FromHours(2);
    private static readonly TimeSpan ValidateTimeout = TimeSpan.FromMinutes(5);

    private readonly IWiimmsToolLocator _toolLocator;
    private readonly IPrivilegedAccessService _privilegedAccessService;
    private readonly IProcessRunner _processRunner;

    public CataloguedWiimmsFormatCommandExecutor(
        IWiimmsToolLocator toolLocator,
        IPrivilegedAccessService privilegedAccessService,
        IProcessRunner processRunner)
    {
        _toolLocator = toolLocator;
        _privilegedAccessService = privilegedAccessService;
        _processRunner = processRunner;
    }

    public bool HasCataloguedFormatWbfsCommand => true;

    public async Task ExecuteFormatWbfsAsync(
        UsbFormatPreview preview,
        IProgress<UsbFormatProgress>? progress,
        CancellationToken cancellationToken)
    {
        var target = ResolveTarget(preview);
        var wwtPath = await _toolLocator.FindWwtAsync(cancellationToken);
        var arguments = CreateFormatArguments(target);

        progress?.Report(new UsbFormatProgress(null, UsbFormatWorkflowState.Running, "Formatando WBFS com comando catalogado.", IsIndeterminate: true));
        var result = await _privilegedAccessService.RunKnownToolAsync(
            new PrivilegedToolRequest(
                "wwt",
                wwtPath,
                arguments,
                FormatTimeout,
                PrivilegedDeviceOperation.FormatWbfsPartition,
                DevicePath: target),
            cancellationToken);

        if (!result.IsSuccess)
        {
            throw new WiimmsCommandException(
                "wwt FORMAT failed.",
                result.ExitCode,
                result.StandardError.Length == 0 ? result.AccessStatus.DiagnosticDetails ?? result.AccessStatus.Message ?? "wwt FORMAT failed." : result.StandardError);
        }

        progress?.Report(new UsbFormatProgress(70, UsbFormatWorkflowState.ValidatingResult, "WBFS formatado. Validando resultado.", IsIndeterminate: false));
    }

    public async Task<bool> ValidateWbfsAsync(UsbFormatPreview preview, CancellationToken cancellationToken)
    {
        var target = ResolveTarget(preview);
        var wwtPath = await _toolLocator.FindWwtAsync(cancellationToken);
        var check = await _processRunner.RunAsync(
            new ProcessRequest(wwtPath, ["--part", target, "CHECK"], ValidateTimeout),
            cancellationToken);
        if (!check.IsSuccess)
        {
            return false;
        }

        var list = await _processRunner.RunAsync(
            new ProcessRequest(wwtPath, ["--part", target, "LIST", "--long", "--no-header"], ValidateTimeout),
            cancellationToken);
        if (!list.IsSuccess || ContainsWbfsError(list))
        {
            return false;
        }

        var space = await _processRunner.RunAsync(
            new ProcessRequest(wwtPath, ["--part", target, "SPACE"], ValidateTimeout),
            cancellationToken);

        return space.IsSuccess && !ContainsWbfsError(check) && !ContainsWbfsError(space);
    }

    private static IReadOnlyList<string> CreateFormatArguments(string target) =>
    [
        "--force",
        "--verbose",
        "--progress",
        "FORMAT",
        target
    ];

    private static string ResolveTarget(UsbFormatPreview preview)
    {
        if (string.IsNullOrWhiteSpace(preview.PartitionPath))
        {
            throw new InvalidOperationException("WBFS format target partition path is missing.");
        }

        if (!IsAllowedTarget(preview.PartitionPath))
        {
            throw new InvalidOperationException("WBFS format target path is not allowed.");
        }

        return preview.PartitionPath;
    }

    private static bool IsAllowedTarget(string target)
    {
        if (target.Contains('\0')
            || target.Contains("..", StringComparison.Ordinal)
            || target.Contains("&&", StringComparison.Ordinal)
            || target.Contains('|')
            || target.Contains(';')
            || target.Contains('>')
            || target.Contains('<')
            || target.Contains('`')
            || target.Contains("$(", StringComparison.Ordinal))
        {
            return false;
        }

        return target.StartsWith("/dev/sd", StringComparison.OrdinalIgnoreCase)
            || target.StartsWith("/dev/disk/by-id/", StringComparison.OrdinalIgnoreCase)
            || target.StartsWith("/dev/disk", StringComparison.OrdinalIgnoreCase)
            || target.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsWbfsError(ProcessResult result)
    {
        return result.StandardOutput.Contains("error", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("error", StringComparison.OrdinalIgnoreCase)
            || result.StandardOutput.Contains("failed", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("failed", StringComparison.OrdinalIgnoreCase);
    }
}
