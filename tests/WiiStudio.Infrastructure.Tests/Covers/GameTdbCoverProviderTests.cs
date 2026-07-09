using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Covers;

namespace WiiStudio.Infrastructure.Tests.Covers;

public sealed class GameTdbCoverProviderTests
{
    [Fact]
    public async Task DownloadAsync_ShouldReturnFirstValidImageFromFallbackUrls()
    {
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/PT/", StringComparison.Ordinal)
                ? new HttpResponseMessage(HttpStatusCode.NotFound)
                : CreateImageResponse());
        var provider = CreateProvider(handler);

        await using var stream = await provider.DownloadAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        stream.Should().NotBeNull();
        handler.Requests.Select(request => request.RequestUri!.ToString()).Should().Equal(
            "https://art.gametdb.com/wii/cover/PT/RMCP01.png",
            "https://art.gametdb.com/wii/cover/EN/RMCP01.png");
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnNullForMissingBannerTemplate()
    {
        var handler = new FakeHttpMessageHandler(_ => CreateImageResponse());
        var provider = CreateProvider(handler);

        var stream = await provider.DownloadAsync("RMCP01", CoverAssetKind.Banner, CancellationToken.None);

        stream.Should().BeNull();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnNullForHtmlResponse()
    {
        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent("<html>not found</html>");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        });
        var provider = CreateProvider(handler);

        var stream = await provider.DownloadAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        stream.Should().BeNull();
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnNullForInvalidImagePayload()
    {
        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent("not-image"u8.ToArray());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return response;
        });
        var provider = CreateProvider(handler);

        var stream = await provider.DownloadAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        stream.Should().BeNull();
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnNullWhenResponseExceedsLimit()
    {
        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(new byte[32]);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return response;
        });
        var provider = CreateProvider(handler, new GameTdbCoverProviderOptions { MaxImageBytes = 8 });

        var stream = await provider.DownloadAsync("RMCP01", CoverAssetKind.FrontCover, CancellationToken.None);

        stream.Should().BeNull();
    }

    private static GameTdbCoverProvider CreateProvider(
        FakeHttpMessageHandler handler,
        GameTdbCoverProviderOptions? options = null)
    {
        options ??= new GameTdbCoverProviderOptions
        {
            FallbackLanguages = ["PT", "EN"]
        };

        return new GameTdbCoverProvider(
            new HttpClient(handler),
            new GameTdbCoverUrlBuilder(options),
            new ImageValidator(),
            options);
    }

    private static HttpResponseMessage CreateImageResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new ByteArrayContent(PngBytes());
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return response;
    }

    private static byte[] PngBytes()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=");
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_handler(request));
        }
    }
}
