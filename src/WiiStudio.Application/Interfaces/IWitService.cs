using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Interfaces;

public interface IWitService
{
    Task<Version> GetVersionAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken);

    Task VerifyAsync(string sourcePath, CancellationToken cancellationToken);

    Task CopyAsync(string sourcePath, string destinationPath, FileFormat targetFormat, CancellationToken cancellationToken);
}
