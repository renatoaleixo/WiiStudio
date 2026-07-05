namespace WiiStudio.Domain.Events;

public sealed record ConversionStarted(Guid ConversionJobId, DateTimeOffset OccurredAt) : IDomainEvent;
