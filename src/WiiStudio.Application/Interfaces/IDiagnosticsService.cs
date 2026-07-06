using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDiagnosticsService
{
    Task<DiagnosticsReport> RunAsync(CancellationToken cancellationToken);

    Task<DiagnosticsReport> RunAsync(DiagnosticsOptions options, CancellationToken cancellationToken);

    Task ExportAsync(string path, CancellationToken cancellationToken);

    Task ExportAsync(DiagnosticsReport report, string outputFile, CancellationToken cancellationToken);
}
