using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IEmulatorLaunchHistoryRepository
{
    Task AppendAsync(EmulatorLaunchHistoryEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmulatorLaunchHistoryEntry>> GetRecentAsync(
        string? gameId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
