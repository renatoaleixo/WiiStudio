namespace WiiStudio.Application.DTOs;

public sealed record DeviceManagerSnapshot(
    IReadOnlyList<DeviceInfo> Devices,
    DeviceManagerState State,
    DateTimeOffset RefreshedAt);
