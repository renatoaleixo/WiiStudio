namespace WiiStudio.Infrastructure.Backups;

public sealed class BackupServiceOptions
{
    public string RestorePointDirectory { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "restore-points");

    public string ApplicationVersion { get; init; } = "0.1.0";
}
