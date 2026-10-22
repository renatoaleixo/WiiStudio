namespace WiiStudio.Application.DTOs;

public sealed record EmulatorProfileDto(
    string ProfileId,
    string EmulatorId,
    string Name,
    bool IsDefault,
    string? ExpectedController,
    bool Fullscreen,
    string? GraphicsBackend,
    string? Notes,
    IReadOnlyDictionary<string, string> Options,
    DateTimeOffset UpdatedAt);

public sealed record GameEmulatorProfileAssignmentDto(
    string GameId,
    string EmulatorId,
    string ProfileId,
    DateTimeOffset UpdatedAt);

public sealed record ControllerDetectionResult(
    bool? IsAvailable,
    string Message);
