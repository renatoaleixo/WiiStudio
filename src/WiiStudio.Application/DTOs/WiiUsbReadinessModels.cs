namespace WiiStudio.Application.DTOs;

public enum ReadinessState
{
    Ready = 1,
    Warning = 2,
    NotReady = 3,
    Unknown = 4
}

public sealed record WiiUsbReadinessCheckItem(
    string CheckId,
    string Title,
    ReadinessState State,
    string? Details,
    string? RecommendedAction,
    string? RepairActionId);

public sealed record WiiUsbReadinessResult(
    string DeviceId,
    ReadinessState State,
    IReadOnlyList<WiiUsbReadinessCheckItem> Items,
    DateTimeOffset CheckedAt);
