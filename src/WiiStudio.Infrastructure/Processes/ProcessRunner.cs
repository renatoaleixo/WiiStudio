using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WiiStudio.Infrastructure.Security;

namespace WiiStudio.Infrastructure.Processes;

public sealed class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;
    private readonly ISafePathService _safePathService;
    private readonly ILogRedactionService _logRedactionService;

    public ProcessRunner(
        ILogger<ProcessRunner> logger,
        ISafePathService? safePathService = null,
        ILogRedactionService? logRedactionService = null)
    {
        _logger = logger;
        _safePathService = safePathService ?? new SafePathService();
        _logRedactionService = logRedactionService ?? new LogRedactionService();
    }

    public async Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        using var timeoutSource = new CancellationTokenSource(request.Timeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

        var stopwatch = Stopwatch.StartNew();
        using var process = CreateProcess(request);

        _logger.LogInformation("Starting process {ExecutablePath}", _logRedactionService.Redact(request.ExecutablePath));

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(linkedSource.Token);
        var standardErrorTask = process.StandardError.ReadToEndAsync(linkedSource.Token);

        try
        {
            await process.WaitForExitAsync(linkedSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillProcess(process);
            throw new TimeoutException($"Process '{request.ExecutablePath}' exceeded timeout {request.Timeout}.");
        }

        stopwatch.Stop();

        var result = new ProcessResult(
            process.ExitCode,
            _logRedactionService.Redact(await standardOutputTask),
            _logRedactionService.Redact(await standardErrorTask),
            stopwatch.Elapsed);

        _logger.LogInformation(
            "Process {ExecutablePath} finished with exit code {ExitCode} in {Duration}ms",
            _logRedactionService.Redact(request.ExecutablePath),
            result.ExitCode,
            result.Duration.TotalMilliseconds);

        return result;
    }

    private void ValidateRequest(ProcessRequest request)
    {
        var executable = _safePathService.ValidateReadableFile(request.ExecutablePath);
        if (!executable.Success)
        {
            throw new FileNotFoundException("Process executable was not found or is not readable.", request.ExecutablePath);
        }

        if (request.Arguments.Any(argument => argument.Contains('\0')))
        {
            throw new ArgumentException("Process arguments contain invalid characters.", nameof(request));
        }
    }

    private static Process CreateProcess(ProcessRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return new Process
        {
            StartInfo = startInfo
        };
    }

    private static void KillProcess(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
    }
}
