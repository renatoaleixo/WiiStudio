namespace WiiStudio.Application.Commands;

public sealed record ImportGameCommand(IReadOnlyList<string> SourcePaths, Guid DestinationDriveId);
