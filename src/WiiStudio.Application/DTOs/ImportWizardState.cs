namespace WiiStudio.Application.DTOs;

public enum ImportWizardState
{
    SelectingFiles,
    Validating,
    SelectingTarget,
    Reviewing,
    Importing,
    Completed,
    Failed,
    Cancelled
}
