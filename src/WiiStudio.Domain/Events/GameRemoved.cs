using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Events;

public sealed record GameRemoved(GameId GameId, DateTimeOffset OccurredAt) : IDomainEvent;
