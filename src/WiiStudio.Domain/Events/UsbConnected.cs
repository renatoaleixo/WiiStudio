namespace WiiStudio.Domain.Events;

public sealed record UsbConnected(Guid UsbDriveId, DateTimeOffset OccurredAt) : IDomainEvent;
