using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Mappers;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Services;

public sealed class GameLibraryScanner : IGameLibraryScanner
{
    private readonly IWitService _witService;

    public GameLibraryScanner(IWitService witService)
    {
        _witService = witService;
    }

    public async Task<LibraryScanResult> ScanAsync(UsbDrive drive, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drive);
        cancellationToken.ThrowIfCancellationRequested();

        var games = await _witService.ListGamesAsync(drive, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var duplicateGameIds = games
            .GroupBy(game => game.GameId.Value)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var uniqueGames = games
            .GroupBy(game => game.GameId.Value)
            .Select(group => group.First())
            .OrderBy(game => game.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(GameMapper.ToDto)
            .ToArray();

        return new LibraryScanResult(uniqueGames, duplicateGameIds, DateTimeOffset.UtcNow);
    }
}
