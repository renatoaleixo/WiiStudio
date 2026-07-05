namespace WiiStudio.Application.DTOs;

public sealed record RepairResultItem(
    string GameId,
    RepairAction Action,
    RepairStatus Status,
    string Message);
