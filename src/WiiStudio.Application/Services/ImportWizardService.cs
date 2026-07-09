using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Services;

public sealed class ImportWizardService : IImportWizardService
{
    private readonly ImportGamesUseCase _importGamesUseCase;

    public ImportWizardService(ImportGamesUseCase importGamesUseCase)
    {
        _importGamesUseCase = importGamesUseCase;
    }

    public Task<ImportPreview> AnalyzeAsync(IReadOnlyList<string> files, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(files);
        cancellationToken.ThrowIfCancellationRequested();

        var items = files
            .Select(file => file.Trim())
            .Where(file => !string.IsNullOrWhiteSpace(file))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(AnalyzeFile)
            .ToArray();

        var canImport = items.Length > 0 && items.All(item => item.Status != ImportWizardFileStatus.Error);
        var state = canImport ? ImportWizardState.SelectingTarget : ImportWizardState.Validating;

        return Task.FromResult(new ImportPreview(items, state, canImport, DateTimeOffset.UtcNow));
    }

    public async Task<ImportWizardResult> ExecuteAsync(ImportWizardRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        await _importGamesUseCase.ExecuteAsync(
            new ImportGameCommand(request.SourcePaths, request.DestinationDriveId),
            cancellationToken);

        return new ImportWizardResult(
            ImportWizardState.Completed,
            request.SourcePaths.Count,
            "Importação concluída.",
            DateTimeOffset.UtcNow);
    }

    private static ImportPreviewItem AnalyzeFile(string sourcePath)
    {
        var format = DetectFormat(sourcePath);
        if (format == FileFormat.Unknown)
        {
            return new ImportPreviewItem(
                sourcePath,
                format,
                ImportWizardFileStatus.Error,
                "Formato não suportado.");
        }

        if (!File.Exists(sourcePath))
        {
            return new ImportPreviewItem(
                sourcePath,
                format,
                ImportWizardFileStatus.Error,
                "Arquivo não encontrado.");
        }

        var fileInfo = new FileInfo(sourcePath);
        if (fileInfo.Length == 0)
        {
            return new ImportPreviewItem(
                sourcePath,
                format,
                ImportWizardFileStatus.Error,
                "Arquivo vazio.");
        }

        return new ImportPreviewItem(
            sourcePath,
            format,
            ImportWizardFileStatus.Valid,
            "Pronto para importação.");
    }

    private static FileFormat DetectFormat(string sourcePath)
    {
        if (sourcePath.EndsWith(".nkit.iso", StringComparison.OrdinalIgnoreCase))
        {
            return FileFormat.NKit;
        }

        return Path.GetExtension(sourcePath).ToLowerInvariant() switch
        {
            ".iso" => FileFormat.Iso,
            ".wbfs" => FileFormat.Wbfs,
            ".rvz" => FileFormat.Rvz,
            _ => FileFormat.Unknown
        };
    }
}
