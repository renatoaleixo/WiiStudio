namespace WiiStudio.Application.DTOs;

public sealed record GameDetailsHistoryItem(
    string Operation,
    bool Success,
    string? Message,
    DateTimeOffset CreatedAt);
