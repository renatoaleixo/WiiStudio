using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Covers;

namespace WiiStudio.Infrastructure.Tests.Covers;

public sealed class GameTdbCoverUrlBuilderTests
{
    [Fact]
    public void BuildCandidateUrls_ShouldBuildFrontCoverFallbackUrls()
    {
        var builder = new GameTdbCoverUrlBuilder(new GameTdbCoverProviderOptions
        {
            BaseUrl = "https://art.gametdb.com/wii",
            PreferredLanguage = "pt-BR",
            FallbackLanguages = ["PT", "EN", "US", "ORIG"]
        });

        var urls = builder.BuildCandidateUrls("rmcp01", CoverAssetKind.FrontCover);

        urls.Select(url => url.ToString()).Should().Equal(
            "https://art.gametdb.com/wii/cover/PT/RMCP01.png",
            "https://art.gametdb.com/wii/cover/EN/RMCP01.png",
            "https://art.gametdb.com/wii/cover/US/RMCP01.png",
            "https://art.gametdb.com/wii/cover/ORIG/RMCP01.png");
    }

    [Fact]
    public void BuildCandidateUrls_ShouldMapBackCoverToFullCover()
    {
        var builder = new GameTdbCoverUrlBuilder(new GameTdbCoverProviderOptions());

        var urls = builder.BuildCandidateUrls("RSPE01", CoverAssetKind.BackCover);

        urls[0].ToString().Should().Be("https://art.gametdb.com/wii/coverfull/PT/RSPE01.png");
    }

    [Fact]
    public void BuildCandidateUrls_ShouldMapDiscArt()
    {
        var builder = new GameTdbCoverUrlBuilder(new GameTdbCoverProviderOptions());

        var urls = builder.BuildCandidateUrls("RSPE01", CoverAssetKind.DiscArt);

        urls[0].ToString().Should().Be("https://art.gametdb.com/wii/disc/PT/RSPE01.png");
    }

    [Fact]
    public void BuildCandidateUrls_ShouldReturnNoBannerUrlsWhenBannerTemplateIsMissing()
    {
        var builder = new GameTdbCoverUrlBuilder(new GameTdbCoverProviderOptions());

        var urls = builder.BuildCandidateUrls("RSPE01", CoverAssetKind.Banner);

        urls.Should().BeEmpty();
    }

    [Fact]
    public void BuildCandidateUrls_ShouldRejectInvalidGameId()
    {
        var builder = new GameTdbCoverUrlBuilder(new GameTdbCoverProviderOptions());

        var act = () => builder.BuildCandidateUrls("../bad", CoverAssetKind.FrontCover);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BuildCandidateUrls_ShouldRequireHttpsBaseUrl()
    {
        var builder = new GameTdbCoverUrlBuilder(new GameTdbCoverProviderOptions
        {
            BaseUrl = "http://art.gametdb.com/wii"
        });

        var act = () => builder.BuildCandidateUrls("RMCP01", CoverAssetKind.FrontCover);

        act.Should().Throw<InvalidOperationException>();
    }
}
