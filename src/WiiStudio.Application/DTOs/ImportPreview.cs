namespace WiiStudio.Application.DTOs;

public sealed record ImportPreview(
    IReadOnlyList<ImportPreviewItem> Items,
    ImportWizardState State,
    bool CanImport,
    DateTimeOffset AnalyzedAt);
