namespace WiiStudio.Application.DTOs;

public sealed record ConversionPreview(
    IReadOnlyList<ConversionPreviewItem> Items,
    ConversionWizardState State,
    bool CanConvert,
    long EstimatedOutputSizeInBytes,
    TimeSpan EstimatedDuration,
    DateTimeOffset AnalyzedAt);
