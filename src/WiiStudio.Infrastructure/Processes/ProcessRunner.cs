using Microsoft.Extensions.Logging;
using WiiStudio.Infrastructure.Security;

namespace WiiStudio.Infrastructure.Processes;

public sealed class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;
    private readonly ISafePathService _safePathService;
    private readonly ILogRedactionService _logRedactionService;
    private readonly IProcessExecutor _processExecutor;

    public ProcessRunner(
        ILogger<ProcessRunner> logger,
        ISafePathService? safePathService = null,
        ILogRedactionService? logRedactionService = null,
        IProcessExecutor? processExecutor = null)
    {
        _logger = logger;
        _safePathService = safePathService ?? new SafePathService();
        _logRedactionService = logRedactionService ?? new LogRedactionService();
        _processExecutor = processExecutor ?? new SystemProcessExecutor();
    }

    public async Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        _logger.LogInformation("Starting process {ExecutablePath}", _logRedactionService.Redact(request.ExecutablePath));

        var rawResult = await _processExecutor.ExecuteAsync(request, cancellationToken);

        var result = new ProcessResult(
            rawResult.ExitCode,
            _logRedactionService.Redact(rawResult.StandardOutput),
            _logRedactionService.Redact(rawResult.StandardError),
            rawResult.Duration);

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
}
