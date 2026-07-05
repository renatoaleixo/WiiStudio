namespace WiiStudio.Domain.Events;

public sealed record ConversionCompleted(Guid ConversionJobId, DateTimeOffset OccurredAt) : IDomainEvent;
