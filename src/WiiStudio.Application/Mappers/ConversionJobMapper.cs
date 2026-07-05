using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.Mappers;

public static class ConversionJobMapper
{
    public static ConversionJobDto ToDto(ConversionJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return new ConversionJobDto(
            job.Id,
            job.SourcePath,
            job.DestinationPath,
            job.SourceFormat,
            job.TargetFormat,
            job.Status,
            job.CreatedAt,
            job.FinishedAt,
            job.ErrorMessage);
    }
}
