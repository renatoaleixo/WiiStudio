using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IExportGameService
{
    Task<ExportPreview> AnalyzeAsync(ExportRequest request, CancellationToken cancellationToken);

    Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken cancellationToken);
}
