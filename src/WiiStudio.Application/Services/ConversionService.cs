using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Mappers;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Services;

public sealed class ConversionService : IConversionService
{
    private readonly IValidator<ConvertGameCommand> _validator;
    private readonly IWitService _witService;

    public ConversionService(IValidator<ConvertGameCommand> validator, IWitService witService)
    {
        _validator = validator;
        _witService = witService;
    }

    public async Task<ConversionJobDto> ConvertAsync(ConvertGameCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var job = new ConversionJob(
            Guid.NewGuid(),
            command.SourcePath,
            command.DestinationPath,
            DetectFormat(command.SourcePath),
            command.TargetFormat);

        job.Start();

        try
        {
            await _witService.CopyAsync(command.SourcePath, command.DestinationPath, command.TargetFormat, cancellationToken);
            job.Complete();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            job.Fail(exception.Message);
        }

        return ConversionJobMapper.ToDto(job);
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
