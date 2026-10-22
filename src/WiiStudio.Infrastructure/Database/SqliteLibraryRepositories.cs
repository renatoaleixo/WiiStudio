using System.Text.Json;
using Microsoft.Data.Sqlite;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Validators;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Infrastructure.Database;

public sealed class SqliteGameLibraryRepository : IGameLibraryRepository, ILibraryFileRepository
{
    private readonly SqliteLibraryUnitOfWork _unitOfWork;

    public SqliteGameLibraryRepository(SqliteLibraryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task UpsertGamesAsync(IReadOnlyList<GameDto> games, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        foreach (var game in games)
        {
            await UpsertGameAsync(connection, game, cancellationToken);
            await UpsertFileAsync(connection, game, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GameDto>> GetGamesAsync(CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Region, DiscType, Format, FilePath, FileSizeBytes, ImportedAt, IsFavorite FROM Games ORDER BY Title COLLATE NOCASE;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var games = new List<GameDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            games.Add(ReadGame(reader));
        }

        return games;
    }

    public async Task<GameDto?> GetByGameIdAsync(string gameId, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Region, DiscType, Format, FilePath, FileSizeBytes, ImportedAt, IsFavorite FROM Games WHERE GameId = $gameId LIMIT 1;";
        command.Parameters.AddWithValue("$gameId", GameIdValidation.Normalize(gameId));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadGame(reader) : null;
    }

    public async Task UpsertAsync(GameDto game, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await UpsertFileAsync(connection, game, cancellationToken);
    }

    private static async Task UpsertGameAsync(SqliteConnection connection, GameDto game, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Games (Id, GameId, Title, Region, DiscType, Format, FilePath, FileSizeBytes, ImportedAt, UpdatedAt, IsFavorite)
            VALUES ($id, $gameId, $title, $region, $discType, $format, $filePath, $size, $importedAt, $updatedAt, $favorite)
            ON CONFLICT(Id) DO UPDATE SET
                GameId = excluded.GameId,
                Title = excluded.Title,
                Region = excluded.Region,
                DiscType = excluded.DiscType,
                Format = excluded.Format,
                FilePath = excluded.FilePath,
                FileSizeBytes = excluded.FileSizeBytes,
                UpdatedAt = excluded.UpdatedAt;
            """;
        command.Parameters.AddWithValue("$id", game.Id);
        command.Parameters.AddWithValue("$gameId", game.Id);
        command.Parameters.AddWithValue("$title", game.Title);
        command.Parameters.AddWithValue("$region", game.Region.ToString());
        command.Parameters.AddWithValue("$discType", game.DiscType.ToString());
        command.Parameters.AddWithValue("$format", game.Format.ToString());
        command.Parameters.AddWithValue("$filePath", (object?)game.FilePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$size", game.SizeInBytes);
        command.Parameters.AddWithValue("$importedAt", game.AddedAt.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$favorite", game.IsFavorite ? 1 : 0);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpsertFileAsync(SqliteConnection connection, GameDto game, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(game.FilePath))
        {
            return;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO LibraryFiles (Id, GameId, Path, Format, SizeBytes, LastSeenAt)
            VALUES ($id, $gameId, $path, $format, $size, $lastSeenAt)
            ON CONFLICT(Path) DO UPDATE SET
                GameId = excluded.GameId,
                Format = excluded.Format,
                SizeBytes = excluded.SizeBytes,
                LastSeenAt = excluded.LastSeenAt;
            """;
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("N"));
        command.Parameters.AddWithValue("$gameId", game.Id);
        command.Parameters.AddWithValue("$path", game.FilePath);
        command.Parameters.AddWithValue("$format", game.Format.ToString());
        command.Parameters.AddWithValue("$size", game.SizeInBytes);
        command.Parameters.AddWithValue("$lastSeenAt", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static GameDto ReadGame(SqliteDataReader reader)
    {
        return new GameDto(
            reader.GetString(0),
            reader.GetString(1),
            Enum.Parse<Region>(reader.GetString(2)),
            Enum.Parse<DiscType>(reader.GetString(3)),
            Enum.Parse<FileFormat>(reader.GetString(4)),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
            DateTimeOffset.Parse(reader.GetString(7)),
            reader.GetInt32(8) == 1);
    }
}

public sealed class SqliteGameMetadataRepository : IGameMetadataRepository
{
    private readonly SqliteLibraryUnitOfWork _unitOfWork;

    public SqliteGameMetadataRepository(SqliteLibraryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GameMetadataDto?> GetAsync(string gameId, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT GameId, Title, Region, Subtitle, Developer, Publisher, Year, Description, Players, Rating,
                   LanguagesJson, GenresJson, OnlineFeatures, Compatibility, GameTdbVersion, UpdatedAt
            FROM GameMetadata WHERE GameId = $gameId LIMIT 1;
            """;
        command.Parameters.AddWithValue("$gameId", GameIdValidation.Normalize(gameId));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new GameMetadataDto(
            reader.GetString(0),
            reader.IsDBNull(1) ? reader.GetString(0) : reader.GetString(1),
            reader.IsDBNull(2) ? Region.Unknown : Enum.Parse<Region>(reader.GetString(2)),
            Subtitle: reader.IsDBNull(3) ? null : reader.GetString(3),
            Developer: reader.IsDBNull(4) ? null : reader.GetString(4),
            Publisher: reader.IsDBNull(5) ? null : reader.GetString(5),
            Year: reader.IsDBNull(6) ? null : reader.GetInt32(6),
            Description: reader.IsDBNull(7) ? null : reader.GetString(7),
            Players: reader.IsDBNull(8) ? null : reader.GetInt32(8),
            Rating: reader.IsDBNull(9) ? null : reader.GetString(9),
            Languages: reader.IsDBNull(10) ? null : JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.GetString(10)),
            Genres: reader.IsDBNull(11) ? null : JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.GetString(11)),
            OnlineFeatures: reader.IsDBNull(12) ? null : reader.GetString(12),
            Compatibility: reader.IsDBNull(13) ? null : reader.GetString(13),
            GameTdbVersion: reader.IsDBNull(14) ? null : reader.GetString(14),
            UpdatedAt: reader.IsDBNull(15) ? null : DateTimeOffset.Parse(reader.GetString(15)));
    }

    public async Task UpsertAsync(GameMetadataDto metadata, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO GameMetadata (GameId, Title, Region, Subtitle, Developer, Publisher, Year, Description, Players, Rating, LanguagesJson, GenresJson, OnlineFeatures, Compatibility, GameTdbVersion, UpdatedAt)
            VALUES ($gameId, $title, $region, $subtitle, $developer, $publisher, $year, $description, $players, $rating, $languages, $genres, $online, $compatibility, $version, $updatedAt)
            ON CONFLICT(GameId) DO UPDATE SET
                Title = excluded.Title,
                Region = excluded.Region,
                Subtitle = excluded.Subtitle,
                Developer = excluded.Developer,
                Publisher = excluded.Publisher,
                Year = excluded.Year,
                Description = excluded.Description,
                Players = excluded.Players,
                Rating = excluded.Rating,
                LanguagesJson = excluded.LanguagesJson,
                GenresJson = excluded.GenresJson,
                OnlineFeatures = excluded.OnlineFeatures,
                Compatibility = excluded.Compatibility,
                GameTdbVersion = excluded.GameTdbVersion,
                UpdatedAt = excluded.UpdatedAt;
            """;
        command.Parameters.AddWithValue("$gameId", GameIdValidation.Normalize(metadata.GameId));
        command.Parameters.AddWithValue("$title", metadata.Title);
        command.Parameters.AddWithValue("$region", metadata.Region.ToString());
        command.Parameters.AddWithValue("$subtitle", (object?)metadata.Subtitle ?? DBNull.Value);
        command.Parameters.AddWithValue("$developer", (object?)metadata.Developer ?? DBNull.Value);
        command.Parameters.AddWithValue("$publisher", (object?)metadata.Publisher ?? DBNull.Value);
        command.Parameters.AddWithValue("$year", (object?)metadata.Year ?? DBNull.Value);
        command.Parameters.AddWithValue("$description", (object?)metadata.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$players", (object?)metadata.Players ?? DBNull.Value);
        command.Parameters.AddWithValue("$rating", (object?)metadata.Rating ?? DBNull.Value);
        command.Parameters.AddWithValue("$languages", metadata.Languages is null ? DBNull.Value : JsonSerializer.Serialize(metadata.Languages));
        command.Parameters.AddWithValue("$genres", metadata.Genres is null ? DBNull.Value : JsonSerializer.Serialize(metadata.Genres));
        command.Parameters.AddWithValue("$online", (object?)metadata.OnlineFeatures ?? DBNull.Value);
        command.Parameters.AddWithValue("$compatibility", (object?)metadata.Compatibility ?? DBNull.Value);
        command.Parameters.AddWithValue("$version", (object?)metadata.GameTdbVersion ?? DBNull.Value);
        command.Parameters.AddWithValue("$updatedAt", (metadata.UpdatedAt ?? DateTimeOffset.UtcNow).ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(string gameId, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM GameMetadata WHERE GameId = $gameId;";
        command.Parameters.AddWithValue("$gameId", GameIdValidation.Normalize(gameId));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed class SqliteGameAssetRepository : IGameAssetRepository
{
    private readonly SqliteLibraryUnitOfWork _unitOfWork;

    public SqliteGameAssetRepository(SqliteLibraryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task UpsertAsync(AssetInfo asset, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO GameAssets (Id, GameId, AssetKind, FilePath, Source, SizeBytes, UpdatedAt)
            VALUES ($id, $gameId, $kind, $path, $source, $size, $updatedAt)
            ON CONFLICT(GameId, AssetKind) DO UPDATE SET
                FilePath = excluded.FilePath,
                Source = excluded.Source,
                SizeBytes = excluded.SizeBytes,
                UpdatedAt = excluded.UpdatedAt;
            """;
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("N"));
        command.Parameters.AddWithValue("$gameId", GameIdValidation.Normalize(asset.Key.GameId));
        command.Parameters.AddWithValue("$kind", asset.Key.Kind.ToString());
        command.Parameters.AddWithValue("$path", asset.LocalPath);
        command.Parameters.AddWithValue("$source", asset.Source.ToString());
        command.Parameters.AddWithValue("$size", asset.SizeInBytes);
        command.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(AssetKey key, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM GameAssets WHERE GameId = $gameId AND AssetKind = $kind;";
        command.Parameters.AddWithValue("$gameId", GameIdValidation.Normalize(key.GameId));
        command.Parameters.AddWithValue("$kind", key.Kind.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetInfo>> GetByGameIdAsync(string gameId, CancellationToken cancellationToken)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT AssetKind, FilePath, Source, SizeBytes, UpdatedAt FROM GameAssets WHERE GameId = $gameId;";
        command.Parameters.AddWithValue("$gameId", GameIdValidation.Normalize(gameId));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var assets = new List<AssetInfo>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var kind = Enum.Parse<CoverAssetKind>(reader.GetString(0));
            assets.Add(new AssetInfo(
                new AssetKey(GameIdValidation.Normalize(gameId), kind),
                reader.GetString(1),
                reader.GetInt64(3),
                DateTimeOffset.Parse(reader.GetString(4)),
                Enum.Parse<CoverAssetSource>(reader.GetString(2))));
        }

        return assets;
    }
}

public sealed class SqliteImportHistoryRepository : IImportHistoryRepository
{
    private readonly SqliteLibraryUnitOfWork _unitOfWork;

    public SqliteImportHistoryRepository(SqliteLibraryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AppendAsync(ImportHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO ImportHistory (Id, GameId, SourcePath, TargetDeviceId, Operation, Success, Message, CreatedAt)
            VALUES ($id, $gameId, $sourcePath, $targetDeviceId, $operation, $success, $message, $createdAt);
            """;
        command.Parameters.AddWithValue("$id", entry.Id.ToString("N"));
        command.Parameters.AddWithValue("$gameId", (object?)entry.GameId ?? DBNull.Value);
        command.Parameters.AddWithValue("$sourcePath", (object?)entry.SourcePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$targetDeviceId", (object?)entry.TargetDeviceId ?? DBNull.Value);
        command.Parameters.AddWithValue("$operation", entry.Operation);
        command.Parameters.AddWithValue("$success", entry.Success ? 1 : 0);
        command.Parameters.AddWithValue("$message", (object?)entry.Message ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ImportHistoryEntry>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, GameId, SourcePath, TargetDeviceId, Operation, Success, Message, CreatedAt
            FROM ImportHistory
            ORDER BY CreatedAt DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var entries = new List<ImportHistoryEntry>();
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new ImportHistoryEntry(
                Guid.Parse(reader.GetString(0)),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5) == 1,
                reader.IsDBNull(6) ? null : reader.GetString(6),
                DateTimeOffset.Parse(reader.GetString(7))));
        }

        return entries;
    }
}

public sealed class SqliteEmulatorLaunchHistoryRepository : IEmulatorLaunchHistoryRepository
{
    private readonly SqliteLibraryUnitOfWork _unitOfWork;

    public SqliteEmulatorLaunchHistoryRepository(SqliteLibraryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AppendAsync(EmulatorLaunchHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO EmulatorLaunchHistory
                (Id, GameId, EmulatorId, FilePath, StartedAt, ExitCode, Result, ErrorMessage)
            VALUES
                ($id, $gameId, $emulatorId, $filePath, $startedAt, $exitCode, $result, $errorMessage);
            """;
        command.Parameters.AddWithValue("$id", entry.Id.ToString("N"));
        command.Parameters.AddWithValue("$gameId", entry.GameId);
        command.Parameters.AddWithValue("$emulatorId", entry.EmulatorId);
        command.Parameters.AddWithValue("$filePath", (object?)entry.FilePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$startedAt", entry.StartedAt.ToString("O"));
        command.Parameters.AddWithValue("$exitCode", (object?)entry.ExitCode ?? DBNull.Value);
        command.Parameters.AddWithValue("$result", entry.Result);
        command.Parameters.AddWithValue("$errorMessage", (object?)entry.ErrorMessage ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmulatorLaunchHistoryEntry>> GetRecentAsync(
        string? gameId = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, GameId, EmulatorId, FilePath, StartedAt, ExitCode, Result, ErrorMessage
            FROM EmulatorLaunchHistory
            WHERE $gameId IS NULL OR GameId = $gameId
            ORDER BY StartedAt DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$gameId", string.IsNullOrWhiteSpace(gameId) ? DBNull.Value : gameId.Trim().ToUpperInvariant());
        command.Parameters.AddWithValue("$limit", Math.Max(1, limit));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var entries = new List<EmulatorLaunchHistoryEntry>();
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new EmulatorLaunchHistoryEntry(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                DateTimeOffset.Parse(reader.GetString(4)),
                reader.IsDBNull(5) ? null : reader.GetInt32(5),
                reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7)));
        }

        return entries;
    }
}

public sealed class SqliteEmulatorProfileRepository : IEmulatorProfileRepository
{
    private readonly SqliteLibraryUnitOfWork _unitOfWork;

    public SqliteEmulatorProfileRepository(SqliteLibraryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EmulatorProfileDto>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ProfileId, EmulatorId, Name, IsDefault, ExpectedController, Fullscreen,
                   GraphicsBackend, Notes, OptionsJson, UpdatedAt
            FROM EmulatorProfiles
            ORDER BY IsDefault DESC, Name COLLATE NOCASE;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var profiles = new List<EmulatorProfileDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            profiles.Add(ReadProfile(reader));
        }

        return profiles;
    }

    public async Task<EmulatorProfileDto?> GetByIdAsync(string profileId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ProfileId, EmulatorId, Name, IsDefault, ExpectedController, Fullscreen,
                   GraphicsBackend, Notes, OptionsJson, UpdatedAt
            FROM EmulatorProfiles WHERE ProfileId = $profileId;
            """;
        command.Parameters.AddWithValue("$profileId", profileId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadProfile(reader) : null;
    }

    public async Task UpsertAsync(EmulatorProfileDto profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        if (profile.IsDefault)
        {
            await using var clearDefault = connection.CreateCommand();
            clearDefault.Transaction = transaction;
            clearDefault.CommandText = "UPDATE EmulatorProfiles SET IsDefault = 0 WHERE EmulatorId = $emulatorId;";
            clearDefault.Parameters.AddWithValue("$emulatorId", profile.EmulatorId);
            await clearDefault.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO EmulatorProfiles
                (ProfileId, EmulatorId, Name, IsDefault, ExpectedController, Fullscreen, GraphicsBackend, Notes, OptionsJson, UpdatedAt)
            VALUES
                ($profileId, $emulatorId, $name, $isDefault, $expectedController, $fullscreen, $graphicsBackend, $notes, $optionsJson, $updatedAt)
            ON CONFLICT(ProfileId) DO UPDATE SET
                EmulatorId = excluded.EmulatorId,
                Name = excluded.Name,
                IsDefault = excluded.IsDefault,
                ExpectedController = excluded.ExpectedController,
                Fullscreen = excluded.Fullscreen,
                GraphicsBackend = excluded.GraphicsBackend,
                Notes = excluded.Notes,
                OptionsJson = excluded.OptionsJson,
                UpdatedAt = excluded.UpdatedAt;
            """;
        command.Parameters.AddWithValue("$profileId", profile.ProfileId);
        command.Parameters.AddWithValue("$emulatorId", profile.EmulatorId);
        command.Parameters.AddWithValue("$name", profile.Name);
        command.Parameters.AddWithValue("$isDefault", profile.IsDefault ? 1 : 0);
        command.Parameters.AddWithValue("$expectedController", (object?)profile.ExpectedController ?? DBNull.Value);
        command.Parameters.AddWithValue("$fullscreen", profile.Fullscreen ? 1 : 0);
        command.Parameters.AddWithValue("$graphicsBackend", (object?)profile.GraphicsBackend ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)profile.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("$optionsJson", JsonSerializer.Serialize(profile.Options));
        command.Parameters.AddWithValue("$updatedAt", profile.UpdatedAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(string profileId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM EmulatorProfiles WHERE ProfileId = $profileId;";
        command.Parameters.AddWithValue("$profileId", profileId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<GameEmulatorProfileAssignmentDto?> GetAssignmentAsync(
        string gameId,
        string emulatorId,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT GameId, EmulatorId, ProfileId, UpdatedAt
            FROM GameEmulatorProfiles
            WHERE GameId = $gameId AND EmulatorId = $emulatorId;
            """;
        command.Parameters.AddWithValue("$gameId", gameId.Trim().ToUpperInvariant());
        command.Parameters.AddWithValue("$emulatorId", emulatorId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new GameEmulatorProfileAssignmentDto(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTimeOffset.Parse(reader.GetString(3)))
            : null;
    }

    public async Task UpsertAssignmentAsync(GameEmulatorProfileAssignmentDto assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        await _unitOfWork.InitializeAsync(cancellationToken);
        await using var connection = _unitOfWork.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO GameEmulatorProfiles (GameId, EmulatorId, ProfileId, UpdatedAt)
            VALUES ($gameId, $emulatorId, $profileId, $updatedAt)
            ON CONFLICT(GameId, EmulatorId) DO UPDATE SET
                ProfileId = excluded.ProfileId,
                UpdatedAt = excluded.UpdatedAt;
            """;
        command.Parameters.AddWithValue("$gameId", assignment.GameId.Trim().ToUpperInvariant());
        command.Parameters.AddWithValue("$emulatorId", assignment.EmulatorId);
        command.Parameters.AddWithValue("$profileId", assignment.ProfileId);
        command.Parameters.AddWithValue("$updatedAt", assignment.UpdatedAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static EmulatorProfileDto ReadProfile(SqliteDataReader reader)
    {
        var options = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(8))
            ?? new Dictionary<string, string>();
        return new EmulatorProfileDto(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3) == 1,
            reader.IsDBNull(4) ? null : reader.GetString(4), reader.GetInt32(5) == 1,
            reader.IsDBNull(6) ? null : reader.GetString(6), reader.IsDBNull(7) ? null : reader.GetString(7),
            options, DateTimeOffset.Parse(reader.GetString(9)));
    }
}
