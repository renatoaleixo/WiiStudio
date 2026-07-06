namespace WiiStudio.Application.DTOs;

public sealed record TaskRequest(
    ManagedTaskType Type,
    string Title,
    string? Description = null,
    ManagedTaskPriority Priority = ManagedTaskPriority.Normal);
