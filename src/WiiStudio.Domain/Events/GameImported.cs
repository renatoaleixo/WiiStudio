using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Events;

public sealed record GameImported(GameId GameId, DateTimeOffset OccurredAt) : IDomainEvent;
