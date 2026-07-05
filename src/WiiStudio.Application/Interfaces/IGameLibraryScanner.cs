using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Interfaces;

public interface IGameLibraryScanner
{
    Task<LibraryScanResult> ScanAsync(UsbDrive drive, CancellationToken cancellationToken);
}
