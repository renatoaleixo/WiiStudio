using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IUsbWriteWorkflowService
{
    Task<UsbWritePreview> CreatePreviewAsync(
        string sourcePath,
        Guid targetDriveId,
        CancellationToken cancellationToken = default);

    Task<UsbWriteResult> ExecuteAsync(
        UsbWriteExecutionRequest request,
        IProgress<WiimmsOperationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
