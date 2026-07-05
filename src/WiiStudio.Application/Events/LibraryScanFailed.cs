namespace WiiStudio.Application.Events;

public sealed record LibraryScanFailed(Guid UsbDriveId, string ErrorMessage, DateTimeOffset OccurredAt);
