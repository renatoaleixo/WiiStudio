namespace WiiStudio.Domain.Events;

public sealed record UsbDisconnected(Guid UsbDriveId, DateTimeOffset OccurredAt) : IDomainEvent;
