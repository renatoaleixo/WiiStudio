namespace WiiStudio.Application.DTOs;

public sealed record DashboardActivity(
    string Title,
    string Detail,
    DateTimeOffset OccurredAt);
