using System.Net;
using System.Security.Cryptography;
using FluentAssertions;
using WiiStudio.Application.ExternalTools;
using WiiStudio.Infrastructure.ExternalTools;

namespace WiiStudio.Infrastructure.Tests.ExternalTools;

public sealed class ExternalToolNetworkDownloaderTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wiistudio-network-downloader-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task DownloadAsync_ShouldBlockHttpUrl()
    {
        var downloader = CreateDownloader("payload");
        var source = Source("http://example.test/tool.zip", Sha256("payload"));

        var act = () => downloader.DownloadAsync(source, null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*httpsRequired*");
    }

    [Fact]
    public async Task DownloadAsync_ShouldRequireChecksum()
    {
        var downloader = CreateDownloader("payload");
        var source = Source("https://example.test/tool.zip", string.Empty);

        var act = () => downloader.DownloadAsync(source, null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*checksumRequired*");
    }

    [Fact]
    public async Task DownloadAsync_ShouldDownloadHttpsPackageAndReportProgress()
    {
        var payload = "payload";
        var downloader = CreateDownloader(payload);
        var source = Source("https://example.test/tool.zip", Sha256(payload));
        var progressItems = new List<ExternalToolDownloadProgress>();

        var package = await downloader.DownloadAsync(
            source,
            new Progress<ExternalToolDownloadProgress>(progressItems.Add),
            CancellationToken.None);

        File.Exists(package.FilePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(package.FilePath);
        content.Should().Be(payload);
        progressItems.Should().Contain(item => item.State == ExternalToolDownloadState.Downloading);
        progressItems.Should().Contain(item => item.State == ExternalToolDownloadState.ChecksumValid);
    }

    [Fact]
    public async Task DownloadAsync_ShouldDeletePackageWhenChecksumIsInvalid()
    {
        var downloader = CreateDownloader("payload");
        var source = Source("https://example.test/tool.zip", new string('0', 64));

        var act = () => downloader.DownloadAsync(source, null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*checksumInvalid*");
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Should().BeEmpty();
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private ExternalToolNetworkDownloader CreateDownloader(string payload)
    {
        var client = new HttpClient(new FakeHttpHandler(payload));
        return new ExternalToolNetworkDownloader(
            client,
            new ChecksumValidator(),
            new ExternalToolOptions { TemporaryDirectory = root, MaxDownloadBytes = 1024 });
    }

    private static ExternalToolPackageSource Source(string url, string sha256)
    {
        return new ExternalToolPackageSource(
            url,
            sha256,
            "1.0",
            "test",
            IsTrusted: true,
            AutoDownloadEnabled: true);
    }

    private static string Sha256(string value)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly string payload;

        public FakeHttpHandler(string payload)
        {
            this.payload = payload;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent(payload)
            };
            return Task.FromResult(response);
        }
    }
}
