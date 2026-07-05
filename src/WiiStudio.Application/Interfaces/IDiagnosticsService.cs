using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDiagnosticsService
{
    Task<DiagnosticsReport> RunAsync(CancellationToken cancellationToken);

    Task ExportAsync(string path, CancellationToken cancellationToken);
}
