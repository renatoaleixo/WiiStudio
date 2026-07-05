using System.Collections.Concurrent;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class GameLibraryService : IGameLibraryService
{
    private readonly IUsbDriveService _usbDriveService;
    private readonly IGameLibraryScanner _gameLibraryScanner;
    private readonly ConcurrentDictionary<Guid, LibraryScanResult> _cache = new();

    public GameLibraryService(IUsbDriveService usbDriveService, IGameLibraryScanner gameLibraryScanner)
    {
        _usbDriveService = usbDriveService;
        _gameLibraryScanner = gameLibraryScanner;
    }

    public Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (usbDriveId is not null)
        {
            var games = _cache.TryGetValue(usbDriveId.Value, out var result)
                ? result.Games
                : [];

            return Task.FromResult<IReadOnlyList<GameDto>>(games);
        }

        var allGames = _cache
            .OrderBy(entry => entry.Key)
            .SelectMany(entry => entry.Value.Games)
            .ToArray();

        return Task.FromResult<IReadOnlyList<GameDto>>(allGames);
    }

    public async Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken)
    {
        var drive = await FindDriveAsync(usbDriveId, cancellationToken);
        var result = await _gameLibraryScanner.ScanAsync(drive, cancellationToken);

        _cache[usbDriveId] = result;
    }

    private async Task<Domain.Entities.UsbDrive> FindDriveAsync(Guid driveId, CancellationToken cancellationToken)
    {
        var drives = await _usbDriveService.DetectAsync(cancellationToken);

        return drives.FirstOrDefault(drive => drive.Id == driveId)
            ?? throw new UsbDriveNotFoundException(driveId);
    }
}
