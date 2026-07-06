using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record ImportPreviewItem(
    string SourcePath,
    FileFormat Format,
    ImportWizardFileStatus Status,
    string Message);
