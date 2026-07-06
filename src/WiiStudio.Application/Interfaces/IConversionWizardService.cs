using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IConversionWizardService
{
    Task<ConversionPreview> AnalyzeAsync(ConversionWizardRequest request, CancellationToken cancellationToken);

    Task<ConversionWizardResult> ExecuteAsync(ConversionWizardRequest request, CancellationToken cancellationToken);
}
