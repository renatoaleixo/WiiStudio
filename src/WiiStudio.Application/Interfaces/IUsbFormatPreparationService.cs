using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IUsbFormatPreparationService
{
    Task<UsbFormatPreview> CreatePreviewAsync(
        UsbFormatPreparationRequest request,
        CancellationToken cancellationToken = default);

    Task<UsbFormatPreparationResult> ExecuteAsync(
        UsbFormatExecutionRequest request,
        IProgress<UsbFormatProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
