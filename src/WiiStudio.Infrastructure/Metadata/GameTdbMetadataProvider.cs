using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;

namespace WiiStudio.Infrastructure.Metadata;

public sealed class GameTdbMetadataProvider : IGameMetadataProvider
{
    private readonly IGameTdbDatabaseService _databaseService;
    private readonly IGameTdbXmlParser _xmlParser;
    private readonly IGameTdbMetadataMapper _metadataMapper;
    private readonly GameTdbMetadataProviderOptions _options;

    public GameTdbMetadataProvider(
        IGameTdbDatabaseService databaseService,
        IGameTdbXmlParser xmlParser,
        IGameTdbMetadataMapper metadataMapper,
        GameTdbMetadataProviderOptions options)
    {
        _databaseService = databaseService;
        _xmlParser = xmlParser;
        _metadataMapper = metadataMapper;
        _options = options;
    }

    public async Task<GameMetadataDto?> GetByGameIdAsync(string gameId, CancellationToken cancellationToken)
    {
        var normalizedGameId = GameIdValidation.Normalize(gameId);
        var status = await _databaseService.EnsureDatabaseAsync(forceRefresh: false, cancellationToken);
        if (!status.IsAvailable)
        {
            return null;
        }

        await using var xmlStream = await _databaseService.OpenDatabaseXmlAsync(cancellationToken);
        var entry = await _xmlParser.FindByGameIdAsync(xmlStream, normalizedGameId, cancellationToken);
        if (entry is null)
        {
            return null;
        }

        return _metadataMapper.Map(
            entry,
            new GameTdbMetadataMappingContext(
                _options.PreferredLanguage,
                _options.FallbackLanguages,
                status.SourceVersion ?? status.DownloadedAt?.ToString("O") ?? DateTimeOffset.UtcNow.ToString("O"),
                DateTimeOffset.UtcNow));
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            File.Exists(Path.Combine(_options.DatabaseDirectory, "wiitdb.xml")) ||
            (_options.AllowAutomaticMetadataDownload &&
                Uri.TryCreate(_options.DatabaseUrl, UriKind.Absolute, out var uri) &&
                uri.Scheme == Uri.UriSchemeHttps));
    }
}
