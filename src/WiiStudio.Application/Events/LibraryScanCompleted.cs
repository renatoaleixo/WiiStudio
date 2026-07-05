namespace WiiStudio.Application.Events;

public sealed record LibraryScanCompleted(Guid UsbDriveId, int TotalGames, DateTimeOffset OccurredAt);
