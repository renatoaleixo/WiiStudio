namespace WiiStudio.Application.DTOs;

public sealed record ImportWizardResult(
    ImportWizardState State,
    int ImportedCount,
    string Message,
    DateTimeOffset CompletedAt);
