namespace WiiStudio.Application.DTOs;

public sealed record GameDetailsActionAvailability(
    GameDetailsAction Action,
    string DisplayName,
    bool IsAvailable,
    string? UnavailableReason = null);
