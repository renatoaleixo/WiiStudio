using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IWiiUsbReadinessService
{
    Task<WiiUsbReadinessResult> CheckAsync(
        Guid driveId,
        CancellationToken cancellationToken = default);

    Task ExportAsync(
        WiiUsbReadinessResult result,
        string outputFile,
        CancellationToken cancellationToken = default);
}
