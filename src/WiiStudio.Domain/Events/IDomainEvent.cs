namespace WiiStudio.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
