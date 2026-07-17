namespace WiiStudio.Application.DTOs;

public enum DiagnosticRepairRiskLevel
{
    Safe = 1,
    RequiresConfirmation = 2,
    Elevated = 3,
    Destructive = 4
}

public sealed record DiagnosticRepairAction(
    string ActionId,
    string Title,
    string Description,
    bool RequiresConfirmation,
    bool IsDestructive,
    DiagnosticRepairRiskLevel RiskLevel);

public sealed record DiagnosticRepairRequest(
    string ActionId,
    bool UserConfirmed = false,
    string? CheckName = null);

public sealed record DiagnosticRepairResult(
    string ActionId,
    bool Success,
    string Message,
    DiagnosticsReport? RefreshedReport = null);

public sealed record DiagnosticRepairHistoryEntry(
    Guid Id,
    string ActionId,
    string Title,
    bool Success,
    string Message,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    string? CheckName = null);
