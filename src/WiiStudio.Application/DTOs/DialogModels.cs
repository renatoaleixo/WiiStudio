namespace WiiStudio.Application.DTOs;

public enum DialogResult
{
    None,
    Confirmed,
    Cancelled,
    Closed,
    Failed
}

public enum DialogKind
{
    Information,
    Confirmation,
    DangerConfirmation,
    Input,
    Form,
    Progress,
    Error,
    AdvancedDetails
}

public sealed record DialogRequest(
    DialogKind Kind,
    string Title,
    string Message,
    string? Details = null,
    string PrimaryActionText = "Confirmar",
    string SecondaryActionText = "Cancelar",
    bool RequiresExplicitConfirmation = false,
    string? ExplicitConfirmationText = null);

public sealed record DialogProgress(
    string Operation,
    double? Percent,
    bool IsIndeterminate,
    bool CanCancel,
    TimeSpan? Elapsed = null,
    string? Summary = null);
