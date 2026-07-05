using System.Text;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Services;

public sealed class ExportGameService : IExportGameService
{
    private readonly IGameLibraryService _gameLibraryService;
    private readonly IWitService _witService;
    private readonly IExportFileSystem _fileSystem;

    public ExportGameService(IGameLibraryService gameLibraryService, IWitService witService, IExportFileSystem fileSystem)
    {
        _gameLibraryService = gameLibraryService;
        _witService = witService;
        _fileSystem = fileSystem;
    }

    public async Task<ExportPreview> AnalyzeAsync(ExportRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        cancellationToken.ThrowIfCancellationRequested();

        var games = await _gameLibraryService.GetGamesAsync(null, cancellationToken);
        var items = new List<ExportPreviewItem>();

        foreach (var gameId in request.GameIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var game = FindGame(games, gameId);
            var sourcePath = GetSourcePath(game);
            var destinationPath = ResolveDestinationPath(request, game);
            var hasConflict = _fileSystem.FileExists(destinationPath);
            var willExport = request.ConflictResolution switch
            {
                ExportConflictResolution.Skip when hasConflict => false,
                ExportConflictResolution.Cancel when hasConflict => false,
                _ => true
            };

            items.Add(new ExportPreviewItem(game.Id, game.Title, sourcePath, destinationPath, hasConflict, willExport));
        }

        return new ExportPreview(items);
    }

    public async Task<ExportResult> ExportAsync(ExportRequest request, CancellationToken cancellationToken)
    {
        var preview = await AnalyzeAsync(request, cancellationToken);
        if (preview.HasConflicts && request.ConflictResolution == ExportConflictResolution.Cancel)
        {
            throw new InvalidOperationException("Export contains file conflicts and conflict resolution is cancel.");
        }

        _fileSystem.EnsureDirectoryExists(request.DestinationDirectory);
        var results = new List<ExportResultItem>();

        foreach (var item in preview.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!item.WillExport)
            {
                results.Add(new ExportResultItem(item.GameId, ExportStatus.Skipped, item.DestinationPath, "File conflict skipped."));
                continue;
            }

            try
            {
                await _witService.CopyAsync(item.SourcePath, item.DestinationPath, request.TargetFormat, cancellationToken);
                results.Add(new ExportResultItem(item.GameId, ExportStatus.Exported, item.DestinationPath));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                results.Add(new ExportResultItem(item.GameId, ExportStatus.Failed, item.DestinationPath, exception.Message));
            }
        }

        return new ExportResult(results, DateTimeOffset.UtcNow);
    }

    private static void ValidateRequest(ExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DestinationDirectory);

        if (request.GameIds.Count == 0)
        {
            throw new ArgumentException("At least one game must be selected for export.", nameof(request));
        }

        if (request.TargetFormat is not FileFormat.Iso and not FileFormat.Wbfs)
        {
            throw new ArgumentException("Only ISO and WBFS exports are currently supported.", nameof(request));
        }
    }

    private static GameDto FindGame(IReadOnlyList<GameDto> games, string gameId)
    {
        if (!GameIdValidation.IsValid(gameId))
        {
            throw new ArgumentException("GameId must contain exactly 6 letters or digits.", nameof(gameId));
        }

        var normalizedGameId = gameId.Trim().ToUpperInvariant();

        return games.FirstOrDefault(game => string.Equals(game.Id, normalizedGameId, StringComparison.OrdinalIgnoreCase))
            ?? throw new GameNotFoundException(normalizedGameId);
    }

    private static string GetSourcePath(GameDto game)
    {
        if (string.IsNullOrWhiteSpace(game.FilePath))
        {
            throw new InvalidOperationException($"Game '{game.Id}' does not have a file path.");
        }

        return game.FilePath;
    }

    private string ResolveDestinationPath(ExportRequest request, GameDto game)
    {
        var extension = request.TargetFormat == FileFormat.Iso ? ".iso" : ".wbfs";
        var fileName = $"{SanitizeFileName(game.Title)} [{game.Id}]{extension}";
        var destinationPath = Path.Combine(request.DestinationDirectory, fileName);

        if (request.ConflictResolution != ExportConflictResolution.Rename)
        {
            return destinationPath;
        }

        var attempt = 1;
        while (_fileSystem.FileExists(destinationPath))
        {
            destinationPath = Path.Combine(request.DestinationDirectory, $"{SanitizeFileName(game.Title)} [{game.Id}] ({attempt}){extension}");
            attempt++;
        }

        return destinationPath;
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(invalidChars.Contains(character) ? '_' : character);
        }

        return builder.ToString().Trim();
    }
}
