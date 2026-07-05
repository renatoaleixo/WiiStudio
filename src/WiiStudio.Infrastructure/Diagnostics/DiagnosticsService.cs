using System.Text.Json;
using Microsoft.Extensions.Logging;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Diagnostics;

public sealed class DiagnosticsService : IDiagnosticsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly IWitService _witService;
    private readonly IWiimmsToolLocator _toolLocator;
    private readonly ILogger<DiagnosticsService> _logger;

    public DiagnosticsService(
        IWitService witService,
        IWiimmsToolLocator toolLocator,
        ILogger<DiagnosticsService> logger)
    {
        _witService = witService;
        _toolLocator = toolLocator;
        _logger = logger;
    }

    public async Task<DiagnosticsReport> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var checks = new List<DiagnosticsCheckResult>
        {
            await CheckWitAsync(cancellationToken),
            await CheckWwtAsync(cancellationToken)
        };

        var report = new DiagnosticsReport(DateTimeOffset.UtcNow, checks);
        _logger.LogInformation("Diagnostics completed with {CheckCount} check(s)", checks.Count);

        return report;
    }

    public async Task ExportAsync(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();

        var report = await RunAsync(cancellationToken);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, report, JsonOptions, cancellationToken);

        _logger.LogInformation("Diagnostics report exported to {Path}", path);
    }

    private async Task<DiagnosticsCheckResult> CheckWitAsync(CancellationToken cancellationToken)
    {
        try
        {
            var version = await _witService.GetVersionAsync(cancellationToken);

            return new DiagnosticsCheckResult("wit", DiagnosticsStatus.Healthy, "wit is available.", version.ToString());
        }
        catch (WiimmsNotInstalledException exception)
        {
            return Unhealthy(exception.ToolName, "wit was not found.", exception);
        }
        catch (WiimmsVersionParseException exception)
        {
            return Unhealthy("wit", "wit version could not be parsed.", exception);
        }
        catch (WiimmsCommandException exception)
        {
            return Unhealthy("wit", "wit version command failed.", exception);
        }
    }

    private async Task<DiagnosticsCheckResult> CheckWwtAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _toolLocator.FindWwtAsync(cancellationToken);

            return new DiagnosticsCheckResult("wwt", DiagnosticsStatus.Healthy, "wwt is available.");
        }
        catch (WiimmsNotInstalledException exception)
        {
            return Unhealthy(exception.ToolName, "wwt was not found.", exception);
        }
    }

    private DiagnosticsCheckResult Unhealthy(string name, string message, Exception exception)
    {
        _logger.LogWarning(exception, "Diagnostics check {CheckName} failed", name);

        return new DiagnosticsCheckResult(name, DiagnosticsStatus.Unhealthy, message);
    }
}
