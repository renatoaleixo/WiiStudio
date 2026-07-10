using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class LogEntryViewModel
{
    public LogEntryViewModel(LogEntry entry)
    {
        Timestamp = entry.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        Level = entry.Level.ToString();
        Source = entry.Source;
        Category = entry.Category;
        Message = entry.Message;
        OperationId = entry.OperationId ?? "-";
        Exception = entry.Exception ?? string.Empty;
        HasException = !string.IsNullOrWhiteSpace(entry.Exception);
    }

    public string Timestamp { get; }
    public string Level { get; }
    public string Source { get; }
    public string Category { get; }
    public string Message { get; }
    public string OperationId { get; }
    public string Exception { get; }
    public bool HasException { get; }
}
