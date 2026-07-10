namespace WiiStudio.Infrastructure.Database;

public sealed class LibraryDatabaseOptions
{
    public string DatabasePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "database",
        "wiistudio.db");

    public bool EnableWal { get; init; } = true;

    public bool BackupBeforeMigration { get; init; } = true;
}
