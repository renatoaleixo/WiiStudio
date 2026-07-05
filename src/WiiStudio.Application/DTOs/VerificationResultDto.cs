namespace WiiStudio.Application.DTOs;

public sealed record VerificationResultDto(
    string GameId,
    VerificationStatus Status,
    string Message,
    string? Details = null);
