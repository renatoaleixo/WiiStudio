using Microsoft.Data.Sqlite;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Database;

public sealed class SqliteLibraryUnitOfWork : ILibraryUnitOfWork
{
    private readonly LibraryDatabaseOptions _options;
    private bool _initialized;

    public SqliteLibraryUnitOfWork(LibraryDatabaseOptions options)
    {
        _options = options;
    }

    public string ConnectionString => new SqliteConnectionStringBuilder
    {
        DataSource = _options.DatabasePath,
        Mode = SqliteOpenMode.ReadWriteCreate
    }.ToString();

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        var directory = Path.GetDirectoryName(_options.DatabasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (_options.BackupBeforeMigration && File.Exists(_options.DatabasePath))
        {
            File.Copy(_options.DatabasePath, $"{_options.DatabasePath}.bak", overwrite: true);
        }

        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        if (_options.EnableWal)
        {
            await ExecuteAsync(connection, "PRAGMA journal_mode=WAL;", cancellationToken);
        }

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS SchemaMigrations (
                Version INTEGER PRIMARY KEY,
                AppliedAt TEXT NOT NULL
            );
            """, cancellationToken);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS Games (
                Id TEXT PRIMARY KEY,
                GameId TEXT NOT NULL,
                Title TEXT NOT NULL,
                Region TEXT NULL,
                DiscType TEXT NULL,
                Format TEXT NULL,
                FilePath TEXT NULL,
                FileSizeBytes INTEGER NULL,
                ImportedAt TEXT NULL,
                UpdatedAt TEXT NOT NULL,
                IsFavorite INTEGER NOT NULL DEFAULT 0
            );
            """, cancellationToken);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS GameMetadata (
                GameId TEXT PRIMARY KEY,
                Title TEXT NULL,
                Region TEXT NULL,
                Subtitle TEXT NULL,
                Developer TEXT NULL,
                Publisher TEXT NULL,
                Year INTEGER NULL,
                Description TEXT NULL,
                Players INTEGER NULL,
                Rating TEXT NULL,
                LanguagesJson TEXT NULL,
                GenresJson TEXT NULL,
                OnlineFeatures TEXT NULL,
                Compatibility TEXT NULL,
                GameTdbVersion TEXT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """, cancellationToken);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS GameAssets (
                Id TEXT PRIMARY KEY,
                GameId TEXT NOT NULL,
                AssetKind TEXT NOT NULL,
                FilePath TEXT NOT NULL,
                Source TEXT NULL,
                SizeBytes INTEGER NOT NULL DEFAULT 0,
                UpdatedAt TEXT NOT NULL,
                UNIQUE(GameId, AssetKind)
            );
            """, cancellationToken);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS LibraryFiles (
                Id TEXT PRIMARY KEY,
                GameId TEXT NOT NULL,
                Path TEXT NOT NULL,
                Format TEXT NOT NULL,
                SizeBytes INTEGER NULL,
                Hash TEXT NULL,
                LastSeenAt TEXT NULL,
                UNIQUE(Path)
            );
            """, cancellationToken);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS ImportHistory (
                Id TEXT PRIMARY KEY,
                GameId TEXT NULL,
                SourcePath TEXT NULL,
                TargetDeviceId TEXT NULL,
                Operation TEXT NOT NULL,
                Success INTEGER NOT NULL,
                Message TEXT NULL,
                CreatedAt TEXT NOT NULL
            );
            """, cancellationToken);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS EmulatorLaunchHistory (
                Id TEXT PRIMARY KEY,
                GameId TEXT NOT NULL,
                EmulatorId TEXT NOT NULL,
                FilePath TEXT NULL,
                StartedAt TEXT NOT NULL,
                ExitCode INTEGER NULL,
                Result TEXT NOT NULL,
                ErrorMessage TEXT NULL
            );
            """, cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_Games_GameId ON Games(GameId);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_Games_Title ON Games(Title);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_Games_IsFavorite ON Games(IsFavorite);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_GameAssets_GameId ON GameAssets(GameId);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_LibraryFiles_GameId ON LibraryFiles(GameId);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_ImportHistory_GameId ON ImportHistory(GameId);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_ImportHistory_CreatedAt ON ImportHistory(CreatedAt);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_EmulatorLaunchHistory_GameId ON EmulatorLaunchHistory(GameId);", cancellationToken);
        await ExecuteAsync(connection, "CREATE INDEX IF NOT EXISTS IX_EmulatorLaunchHistory_StartedAt ON EmulatorLaunchHistory(StartedAt);", cancellationToken);
        await ExecuteAsync(connection, "INSERT OR IGNORE INTO SchemaMigrations (Version, AppliedAt) VALUES (1, $appliedAt);", cancellationToken, ("$appliedAt", DateTimeOffset.UtcNow.ToString("O")));
        await ExecuteAsync(connection, "INSERT OR IGNORE INTO SchemaMigrations (Version, AppliedAt) VALUES (2, $appliedAt);", cancellationToken, ("$appliedAt", DateTimeOffset.UtcNow.ToString("O")));
        await ExecuteAsync(connection, "INSERT OR IGNORE INTO SchemaMigrations (Version, AppliedAt) VALUES (3, $appliedAt);", cancellationToken, ("$appliedAt", DateTimeOffset.UtcNow.ToString("O")));

        _initialized = true;
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    private static async Task ExecuteAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken, params (string Name, object? Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
