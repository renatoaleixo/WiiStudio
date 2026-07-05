using WiiStudio.Application.Abstractions;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.UseCases;

public sealed class ListGamesUseCase
{
    private readonly IWitService _witService;

    public ListGamesUseCase(IWitService witService)
    {
        _witService = witService;
    }

    public Task<IReadOnlyList<Game>> ExecuteAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);

        return _witService.ListGamesAsync(drive, cancellationToken);
    }
}
