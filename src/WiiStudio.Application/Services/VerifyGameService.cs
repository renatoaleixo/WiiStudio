using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Application.Services;

public sealed class VerifyGameService : IVerifyGameService
{
    private readonly IGameLibraryService _gameLibraryService;
    private readonly IWitService _witService;

    public VerifyGameService(IGameLibraryService gameLibraryService, IWitService witService)
    {
        _gameLibraryService = gameLibraryService;
        _witService = witService;
    }

    public async Task<VerificationReport> VerifyAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameIds);
        cancellationToken.ThrowIfCancellationRequested();

        var games = await _gameLibraryService.GetGamesAsync(null, cancellationToken);
        var results = new List<VerificationResultDto>();

        foreach (var gameId in gameIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await VerifyGameAsync(gameId, games, cancellationToken));
        }

        return new VerificationReport(results, DateTimeOffset.UtcNow);
    }

    private async Task<VerificationResultDto> VerifyGameAsync(
        string gameId,
        IReadOnlyList<GameDto> games,
        CancellationToken cancellationToken)
    {
        if (!GameIdValidation.IsValid(gameId))
        {
            return new VerificationResultDto(gameId, VerificationStatus.Unsupported, "Game ID is invalid.");
        }

        var normalizedGameId = gameId.Trim().ToUpperInvariant();
        var game = games.FirstOrDefault(candidate => string.Equals(candidate.Id, normalizedGameId, StringComparison.OrdinalIgnoreCase));
        if (game is null)
        {
            return new VerificationResultDto(normalizedGameId, VerificationStatus.Warning, "Game was not found in the library.");
        }

        if (string.IsNullOrWhiteSpace(game.FilePath))
        {
            return new VerificationResultDto(normalizedGameId, VerificationStatus.Unsupported, "Game does not have a file path.");
        }

        try
        {
            await _witService.VerifyAsync(game.FilePath, cancellationToken);

            return new VerificationResultDto(normalizedGameId, VerificationStatus.Healthy, "Game verified successfully.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new VerificationResultDto(
                normalizedGameId,
                VerificationStatus.Corrupted,
                "Game verification failed.",
                exception.Message);
        }
    }
}
