using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IControllerDetectionService
{
    Task<ControllerDetectionResult> CheckAsync(
        string? expectedController,
        CancellationToken cancellationToken = default);
}
