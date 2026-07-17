namespace WiiStudio.Infrastructure.Diagnostics;

public sealed class DiagnosticsRepairHistoryStoreOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "diagnostics",
        "repair-history.json");

    public int HistoryLimit { get; init; } = 500;
}
