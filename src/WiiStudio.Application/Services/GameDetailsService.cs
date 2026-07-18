using WiiStudio.Application.DTOs;
using WiiStudio.Application.Exceptions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class GameDetailsService : IGameDetailsService
{
    private static readonly GameDetailsAction[] DefaultActions =
    [
        GameDetailsAction.Convert,
        GameDetailsAction.Export,
        GameDetailsAction.VerifyIntegrity,
        GameDetailsAction.Repair,
        GameDetailsAction.RefreshCovers,
        GameDetailsAction.ToggleFavorite,
        GameDetailsAction.Remove
    ];

    private readonly IGameLibraryService _gameLibraryService;
    private readonly IFavoriteGameService _favoriteGameService;
    private readonly IMetadataCacheService _metadataCacheService;
    private readonly IGameAssetRepository _gameAssetRepository;
    private readonly IImportHistoryRepository? _importHistoryRepository;

    public GameDetailsService(
        IGameLibraryService gameLibraryService,
        IFavoriteGameService favoriteGameService,
        IMetadataCacheService metadataCacheService,
        IGameAssetRepository gameAssetRepository,
        IImportHistoryRepository? importHistoryRepository = null)
    {
        _gameLibraryService = gameLibraryService;
        _favoriteGameService = favoriteGameService;
        _metadataCacheService = metadataCacheService;
        _gameAssetRepository = gameAssetRepository;
        _importHistoryRepository = importHistoryRepository;
    }

    public async Task<GameDetailsDto> GetAsync(string gameId, CancellationToken cancellationToken)
    {
        ValidateGameId(gameId);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedGameId = gameId.Trim().ToUpperInvariant();
        var games = await _gameLibraryService.GetGamesAsync(null, cancellationToken);
        var game = games.FirstOrDefault(candidate => string.Equals(candidate.Id, normalizedGameId, StringComparison.OrdinalIgnoreCase))
            ?? throw new GameNotFoundException(normalizedGameId);
        var isFavorite = await _favoriteGameService.IsFavoriteAsync(normalizedGameId, cancellationToken);
        var metadata = await _metadataCacheService.GetAsync(normalizedGameId, cancellationToken);
        var assets = await _gameAssetRepository.GetByGameIdAsync(normalizedGameId, cancellationToken);
        var fileExists = string.IsNullOrWhiteSpace(game.FilePath) || File.Exists(game.FilePath);
        var history = await GetHistoryAsync(normalizedGameId, cancellationToken);
        var actionAvailability = CreateActionAvailability(fileExists);
        var gallery = Enum.GetValues<CoverAssetKind>()
            .Select(kind =>
            {
                var asset = assets.FirstOrDefault(candidate => candidate.Key.Kind == kind);
                return new GameDetailAssetDto(
                    kind,
                    GetAssetDisplayName(kind),
                    asset?.LocalPath,
                    asset is not null && File.Exists(asset.LocalPath),
                    asset?.SizeInBytes ?? 0);
            })
            .ToArray();

        return new GameDetailsDto(
            game.Id,
            game.Title,
            game.Region,
            game.DiscType,
            game.Format,
            game.FilePath,
            game.SizeInBytes,
            game.AddedAt,
            isFavorite,
            DefaultActions,
            metadata,
            gallery,
            fileExists,
            actionAvailability,
            history);
    }

    public async Task RefreshAsync(string gameId, CancellationToken cancellationToken)
    {
        await GetAsync(gameId, cancellationToken);
    }

    private static void ValidateGameId(string gameId)
    {
        if (!GameIdValidation.IsValid(gameId))
        {
            throw new ArgumentException("GameId must contain exactly 6 letters or digits.", nameof(gameId));
        }
    }

    private static string GetAssetDisplayName(CoverAssetKind kind)
    {
        return kind switch
        {
            CoverAssetKind.FrontCover => "Capa frontal",
            CoverAssetKind.BackCover => "Capa verso",
            CoverAssetKind.DiscArt => "Disco",
            CoverAssetKind.Banner => "Banner",
            CoverAssetKind.Icon => "Icone",
            _ => kind.ToString()
        };
    }

    private async Task<IReadOnlyList<GameDetailsHistoryItem>> GetHistoryAsync(
        string gameId,
        CancellationToken cancellationToken)
    {
        if (_importHistoryRepository is null)
        {
            return [];
        }

        var entries = await _importHistoryRepository.GetRecentAsync(100, cancellationToken);
        return entries
            .Where(entry => string.Equals(entry.GameId, gameId, StringComparison.OrdinalIgnoreCase))
            .Take(8)
            .Select(entry => new GameDetailsHistoryItem(entry.Operation, entry.Success, entry.Message, entry.CreatedAt))
            .ToArray();
    }

    private static IReadOnlyList<GameDetailsActionAvailability> CreateActionAvailability(bool fileExists)
    {
        return DefaultActions
            .Select(action =>
            {
                var requiresFile = action is GameDetailsAction.Convert
                    or GameDetailsAction.Export
                    or GameDetailsAction.VerifyIntegrity
                    or GameDetailsAction.Repair;
                var available = fileExists || !requiresFile;
                return new GameDetailsActionAvailability(
                    action,
                    GetActionDisplayName(action),
                    available,
                    available ? null : "Arquivo original não encontrado.");
            })
            .ToArray();
    }

    private static string GetActionDisplayName(GameDetailsAction action)
    {
        return action switch
        {
            GameDetailsAction.Convert => "Converter",
            GameDetailsAction.Export => "Exportar",
            GameDetailsAction.VerifyIntegrity => "Verificar",
            GameDetailsAction.Repair => "Reparar",
            GameDetailsAction.RefreshCovers => "Atualizar capas",
            GameDetailsAction.ToggleFavorite => "Favorito",
            GameDetailsAction.Remove => "Remover",
            _ => action.ToString()
        };
    }
}
