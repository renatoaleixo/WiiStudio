namespace WiiStudio.Application.Commands;

public sealed record DownloadCoverCommand(string GameId, bool ForceRefresh);
