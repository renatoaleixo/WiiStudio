namespace WiiStudio.Application.Events;

public sealed record LibraryScanStarted(Guid UsbDriveId, DateTimeOffset OccurredAt);
