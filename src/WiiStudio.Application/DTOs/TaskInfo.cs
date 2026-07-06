namespace WiiStudio.Application.DTOs;

public sealed record TaskInfo(
    Guid Id,
    ManagedTaskType Type,
    ManagedTaskStatus Status,
    string Title,
    string? Description,
    ManagedTaskPriority Priority,
    double Progress,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? FinishedAt = null,
    string? ErrorMessage = null);
