using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IConversionService
{
    Task<ConversionJobDto> ConvertAsync(ConvertGameCommand command, CancellationToken cancellationToken);
}
