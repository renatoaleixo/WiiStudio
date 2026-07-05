namespace WiiStudio.Application.Commands;

public sealed record RemoveGameCommand(string GameId, Guid SourceDriveId, bool Confirmed);
