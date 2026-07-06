namespace WiiStudio.Infrastructure.Logging;

public sealed class LoggingCenterOptions
{
    public string FilePath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WiiStudio",
        "logs",
        "wiistudio.jsonl");

    public int DefaultQueryLimit { get; init; } = 500;
}
