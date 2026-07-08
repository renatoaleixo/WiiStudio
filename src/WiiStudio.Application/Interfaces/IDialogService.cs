using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IDialogService
{
    Task<DialogResult> ShowInformationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);

    Task<DialogResult> ShowConfirmationAsync(
        DialogRequest request,
        CancellationToken cancellationToken = default);

    Task<DialogResult> ShowDangerConfirmationAsync(
        DialogRequest request,
        CancellationToken cancellationToken = default);

    Task<DialogResult> ShowErrorAsync(
        DialogRequest request,
        CancellationToken cancellationToken = default);
}
