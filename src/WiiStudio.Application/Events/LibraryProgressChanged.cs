namespace WiiStudio.Application.Events;

public sealed record LibraryProgressChanged(Guid UsbDriveId, int DiscoveredGames, DateTimeOffset OccurredAt);
