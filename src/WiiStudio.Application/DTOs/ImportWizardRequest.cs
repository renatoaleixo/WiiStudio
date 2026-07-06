namespace WiiStudio.Application.DTOs;

public sealed record ImportWizardRequest(
    IReadOnlyList<string> SourcePaths,
    Guid DestinationDriveId);
