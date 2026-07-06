namespace WiiStudio.Application.DTOs;

public enum ConversionWizardState
{
    SelectingSource,
    Analyzing,
    SelectingTarget,
    Configuring,
    Reviewing,
    Converting,
    Completed,
    Failed,
    Cancelled
}
