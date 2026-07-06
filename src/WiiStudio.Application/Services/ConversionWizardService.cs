using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Services;

public sealed class ConversionWizardService : IConversionWizardService
{
    private readonly IConversionService _conversionService;

    public ConversionWizardService(IConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    public Task<ConversionPreview> AnalyzeAsync(ConversionWizardRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var sources = NormalizeSources(request.SourcePaths);
        var items = sources
            .Select(source => AnalyzeSource(source, request.DestinationDirectory, request.TargetFormat))
            .ToArray();

        var canConvert = items.Length > 0 && items.All(item => item.CanConvert);
        var estimatedSize = items.Where(item => item.CanConvert).LongCount() * 4L * 1024L * 1024L * 1024L;
        var estimatedDuration = TimeSpan.FromMinutes(Math.Max(1, items.Count(item => item.CanConvert) * 8));

        return Task.FromResult(new ConversionPreview(
            items,
            canConvert ? ConversionWizardState.Reviewing : ConversionWizardState.Analyzing,
            canConvert,
            estimatedSize,
            estimatedDuration,
            DateTimeOffset.UtcNow));
    }

    public async Task<ConversionWizardResult> ExecuteAsync(ConversionWizardRequest request, CancellationToken cancellationToken)
    {
        var preview = await AnalyzeAsync(request, cancellationToken);
        if (!preview.CanConvert)
        {
            return new ConversionWizardResult(
                ConversionWizardState.Failed,
                [],
                "Revise as conversões incompatíveis antes de executar.",
                DateTimeOffset.UtcNow);
        }

        var jobs = new List<ConversionJobDto>();
        foreach (var item in preview.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var job = await _conversionService.ConvertAsync(
                new ConvertGameCommand(item.SourcePath, item.DestinationPath, item.TargetFormat),
                cancellationToken);
            jobs.Add(job);
        }

        var state = jobs.All(job => job.Status == ConversionJobStatus.Completed)
            ? ConversionWizardState.Completed
            : ConversionWizardState.Failed;

        return new ConversionWizardResult(
            state,
            jobs,
            state == ConversionWizardState.Completed ? "Conversão concluída." : "Uma ou mais conversões falharam.",
            DateTimeOffset.UtcNow);
    }

    private static ConversionPreviewItem AnalyzeSource(string sourcePath, string destinationDirectory, FileFormat targetFormat)
    {
        var sourceFormat = DetectFormat(sourcePath);
        if (sourceFormat == FileFormat.Unknown)
        {
            return CreateBlockedItem(sourcePath, destinationDirectory, sourceFormat, targetFormat, "Formato de origem não suportado.");
        }

        if (targetFormat is not (FileFormat.Iso or FileFormat.Wbfs))
        {
            return CreateBlockedItem(sourcePath, destinationDirectory, sourceFormat, targetFormat, "Formato de destino não suportado.");
        }

        if (sourceFormat == targetFormat)
        {
            return CreateBlockedItem(sourcePath, destinationDirectory, sourceFormat, targetFormat, "Origem e destino usam o mesmo formato.");
        }

        var pipeline = ResolvePipeline(sourceFormat, targetFormat);
        if (pipeline is null)
        {
            return CreateBlockedItem(sourcePath, destinationDirectory, sourceFormat, targetFormat, "Pipeline incompatível.");
        }

        return new ConversionPreviewItem(
            sourcePath,
            BuildDestinationPath(sourcePath, destinationDirectory, targetFormat),
            sourceFormat,
            targetFormat,
            pipeline,
            CanConvert: true,
            "Pronto para conversão.");
    }

    private static ConversionPreviewItem CreateBlockedItem(
        string sourcePath,
        string destinationDirectory,
        FileFormat sourceFormat,
        FileFormat targetFormat,
        string message)
    {
        return new ConversionPreviewItem(
            sourcePath,
            BuildDestinationPath(sourcePath, destinationDirectory, targetFormat),
            sourceFormat,
            targetFormat,
            "Indisponível",
            CanConvert: false,
            message);
    }

    private static string? ResolvePipeline(FileFormat sourceFormat, FileFormat targetFormat)
    {
        return (sourceFormat, targetFormat) switch
        {
            (FileFormat.Iso, FileFormat.Wbfs) => "Wiimms ISO -> WBFS",
            (FileFormat.Wbfs, FileFormat.Iso) => "Wiimms WBFS -> ISO",
            (FileFormat.Rvz, FileFormat.Iso) => "RVZ Provider -> ISO",
            (FileFormat.Rvz, FileFormat.Wbfs) => "RVZ Provider -> ISO -> Wiimms",
            (FileFormat.NKit, FileFormat.Iso) => "NKit -> ISO",
            (FileFormat.NKit, FileFormat.Wbfs) => "NKit -> ISO -> Wiimms",
            _ => null
        };
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

    private static string BuildDestinationPath(string sourcePath, string destinationDirectory, FileFormat targetFormat)
    {
        var directory = string.IsNullOrWhiteSpace(destinationDirectory)
            ? Path.GetDirectoryName(sourcePath) ?? string.Empty
            : destinationDirectory.Trim();
        var extension = targetFormat == FileFormat.Iso ? ".iso" : ".wbfs";
        var fileName = Path.GetFileNameWithoutExtension(sourcePath);

        if (sourcePath.EndsWith(".nkit.iso", StringComparison.OrdinalIgnoreCase))
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
        }

        return Path.Combine(directory, $"{fileName}{extension}");
    }

    private static IReadOnlyList<string> NormalizeSources(IReadOnlyList<string> sourcePaths)
    {
        return sourcePaths
            .Select(path => path.Trim())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
