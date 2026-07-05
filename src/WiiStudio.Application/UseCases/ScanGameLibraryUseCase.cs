using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.UseCases;

public sealed class ScanGameLibraryUseCase
{
    private readonly IGameLibraryScanner _gameLibraryScanner;

    public ScanGameLibraryUseCase(IGameLibraryScanner gameLibraryScanner)
    {
        _gameLibraryScanner = gameLibraryScanner;
    }

    public Task<LibraryScanResult> ExecuteAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);

        return _gameLibraryScanner.ScanAsync(drive, cancellationToken);
    }
}
