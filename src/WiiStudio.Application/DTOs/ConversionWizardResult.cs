namespace WiiStudio.Application.DTOs;

public sealed record ConversionWizardResult(
    ConversionWizardState State,
    IReadOnlyList<ConversionJobDto> Jobs,
    string Message,
    DateTimeOffset CompletedAt);
