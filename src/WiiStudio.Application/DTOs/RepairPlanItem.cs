namespace WiiStudio.Application.DTOs;

public sealed record RepairPlanItem(
    string GameId,
    RepairAction Action,
    VerificationStatus VerificationStatus,
    string Reason,
    string Risk);
