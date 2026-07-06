using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface ILoggingCenter
{
    Task RecordAsync(LogEntry entry, CancellationToken cancellationToken);

    Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken);

    Task ExportAsync(LogExportOptions options, CancellationToken cancellationToken);

    Task ClearAsync(LogCleanupOptions options, CancellationToken cancellationToken);
}
