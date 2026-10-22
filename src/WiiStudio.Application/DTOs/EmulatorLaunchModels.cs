namespace WiiStudio.Application.DTOs;

public sealed record EmulatorLaunchRequest(
    string GameId,
    string EmulatorId = "dolphin");

public sealed record EmulatorLaunchExecutionRequest(
    string GameId,
    string EmulatorId = "dolphin",
    bool UserConfirmed = true,
    bool SaveHistory = true);

public sealed record EmulatorLaunchPreview(
    bool CanLaunch,
    string? BlockReason,
    string GameId,
    string? GameTitle,
    string? GameFilePath,
    string EmulatorId,
    string? EmulatorVersion,
    string? ProfileId,
    string? ProfileName,
    string? ExpectedController,
    IReadOnlyList<string> Warnings);

public sealed record EmulatorLaunchResult(
    bool Started,
    string GameId,
    string EmulatorId,
    DateTimeOffset StartedAt,
    string? ErrorMessage = null);
