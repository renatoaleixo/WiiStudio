using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class DiagnosticsCheckItemViewModel
{
    public DiagnosticsCheckItemViewModel(
        DiagnosticsCheckResult check,
        DiagnosticRepairAction? repairAction = null,
        DiagnosticRepairHistoryEntry? lastRepair = null)
    {
        Name = check.Name;
        Status = check.Status.ToString();
        Category = check.Category;
        Severity = check.Severity.ToString();
        Message = check.Message;
        Detail = string.IsNullOrWhiteSpace(check.Version) ? check.Message : check.Version;
        RepairActionId = repairAction?.ActionId;
        RepairActionTitle = repairAction?.Title ?? "Corrigir";
        RepairActionDescription = repairAction?.Description ?? string.Empty;
        RepairRiskText = repairAction is null ? string.Empty : CreateRiskText(repairAction);
        LastRepairStatusText = lastRepair is null
            ? string.Empty
            : $"{(lastRepair.Success ? "Último reparo OK" : "Último reparo falhou")} em {lastRepair.FinishedAt.LocalDateTime:g}: {lastRepair.Message}";
    }

    public string Name { get; }

    public string Status { get; }

    public string Category { get; }

    public string Severity { get; }

    public string Message { get; }

    public string Detail { get; }

    public string? RepairActionId { get; }

    public string RepairActionTitle { get; }

    public string RepairActionDescription { get; }

    public string RepairRiskText { get; }

    public string LastRepairStatusText { get; }

    public bool HasRepairAction => !string.IsNullOrWhiteSpace(RepairActionId);

    public bool HasRepairActionDescription => !string.IsNullOrWhiteSpace(RepairActionDescription);

    public bool HasRepairRisk => !string.IsNullOrWhiteSpace(RepairRiskText);

    public bool HasLastRepair => !string.IsNullOrWhiteSpace(LastRepairStatusText);

    private static string CreateRiskText(DiagnosticRepairAction action)
    {
        var risk = action.RiskLevel switch
        {
            DiagnosticRepairRiskLevel.Safe => "Seguro",
            DiagnosticRepairRiskLevel.RequiresConfirmation => "Requer confirmação",
            DiagnosticRepairRiskLevel.Elevated => "Requer autorização do sistema",
            DiagnosticRepairRiskLevel.Destructive => "Ação destrutiva",
            _ => action.RiskLevel.ToString()
        };

        return action.RequiresConfirmation ? $"{risk} · confirmação obrigatória" : risk;
    }
}
