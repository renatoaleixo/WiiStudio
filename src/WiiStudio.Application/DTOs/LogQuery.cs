namespace WiiStudio.Application.DTOs;

public sealed record LogQuery(
    LogLevel? MinimumLevel = null,
    string? Source = null,
    string? Category = null,
    string? SearchText = null,
    string? OperationId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int? Limit = null);
