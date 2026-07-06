using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IImportWizardService
{
    Task<ImportPreview> AnalyzeAsync(IReadOnlyList<string> files, CancellationToken cancellationToken);

    Task<ImportWizardResult> ExecuteAsync(ImportWizardRequest request, CancellationToken cancellationToken);
}
