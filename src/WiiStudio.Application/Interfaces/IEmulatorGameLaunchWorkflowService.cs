using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IEmulatorGameLaunchWorkflowService
{
    Task<EmulatorLaunchPreview> CreatePreviewAsync(
        EmulatorLaunchRequest request,
        CancellationToken cancellationToken = default);

    Task<EmulatorLaunchResult> ExecuteAsync(
        EmulatorLaunchExecutionRequest request,
        CancellationToken cancellationToken = default);
}
