using WiiStudio.Application.DTOs;

namespace WiiStudio.Infrastructure.Metadata;

public interface IGameTdbMetadataMapper
{
    GameMetadataDto Map(GameTdbGameXmlEntry source, GameTdbMetadataMappingContext context);
}
