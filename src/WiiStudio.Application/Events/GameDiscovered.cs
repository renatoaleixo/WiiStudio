namespace WiiStudio.Application.Events;

public sealed record GameDiscovered(string GameId, string Title, DateTimeOffset OccurredAt);
