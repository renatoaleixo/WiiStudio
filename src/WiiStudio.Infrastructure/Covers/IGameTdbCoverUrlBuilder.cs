using WiiStudio.Application.DTOs;

namespace WiiStudio.Infrastructure.Covers;

public interface IGameTdbCoverUrlBuilder
{
    IReadOnlyList<Uri> BuildCandidateUrls(string gameId, CoverAssetKind kind);
}
