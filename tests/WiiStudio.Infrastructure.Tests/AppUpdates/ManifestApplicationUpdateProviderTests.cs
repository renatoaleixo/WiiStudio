using System.Net;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.AppUpdates;

namespace WiiStudio.Infrastructure.Tests.AppUpdates;

public sealed class ManifestApplicationUpdateProviderTests : IDisposable
{
    private readonly string tempRoot = Path.Combine(Path.GetTempPath(), "wiistudio-update-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task CheckAsync_ShouldReturnUpdateFromHttpsManifest()
    {
        var packageBytes = Encoding.UTF8.GetBytes("update package");
        var checksum = Convert.ToHexString(SHA256.HashData(packageBytes)).ToLowerInvariant();
        var http = new FakeHttpMessageHandler();
        http.AddJson("https://updates.example/manifest.json", $$"""
        {
          "version": "1.2.0",
          "channel": "stable",
          "releaseNotes": "Correções e melhorias.",
          "packages": [
            {
              "platform": "linux-x64",
              "url": "https://updates.example/WiiStudio-1.2.0.zip",
              "sha256": "{{checksum}}",
              "signature": "sig"
            }
          ]
        }
        """);

        var provider = CreateProvider(http);

        var update = await provider.CheckAsync("1.1.0", CancellationToken.None);

        update.Should().NotBeNull();
        update!.Version.Should().Be("1.2.0");
        update.DownloadUrl.Should().Be("https://updates.example/WiiStudio-1.2.0.zip");
        update.Checksum.Should().Be(checksum);
    }

    [Fact]
    public async Task DownloadAsync_ShouldValidateSha256AndStorePackage()
    {
        var packageBytes = Encoding.UTF8.GetBytes("update package");
        var checksum = Convert.ToHexString(SHA256.HashData(packageBytes)).ToLowerInvariant();
        var http = new FakeHttpMessageHandler();
        http.AddBytes("https://updates.example/WiiStudio-1.2.0.zip", packageBytes);
        var provider = CreateProvider(http);
        var update = new ApplicationUpdateInfo(
            "1.2.0",
            "1.1.0",
            IsAvailable: true,
            DateTimeOffset.UtcNow,
            "https://updates.example/WiiStudio-1.2.0.zip",
            Checksum: checksum);

        var package = await provider.DownloadAsync(update, CancellationToken.None);

        package.Should().NotBeNull();
        File.Exists(package!.PackagePath).Should().BeTrue();
        package.Checksum.Should().Be(checksum);
    }

    [Fact]
    public async Task CheckAsync_ShouldIgnoreInsecureManifestUrl()
    {
        var provider = new ManifestApplicationUpdateProvider(
            new HttpClient(new FakeHttpMessageHandler()),
            new ManifestApplicationUpdateProviderOptions
            {
                ManifestUrl = "http://updates.example/manifest.json",
                DownloadDirectory = tempRoot
            });

        var update = await provider.CheckAsync("1.0.0", CancellationToken.None);

        update.Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private ManifestApplicationUpdateProvider CreateProvider(FakeHttpMessageHandler handler) =>
        new(
            new HttpClient(handler),
            new ManifestApplicationUpdateProviderOptions
            {
                ManifestUrl = "https://updates.example/manifest.json",
                Platform = "linux-x64",
                DownloadDirectory = tempRoot
            });

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, HttpResponseMessage> responses = new(StringComparer.OrdinalIgnoreCase);

        public void AddJson(string url, string json) =>
            responses[url] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

        public void AddBytes(string url, byte[] bytes) =>
            responses[url] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is not null && responses.TryGetValue(request.RequestUri.ToString(), out var response))
            {
                return Task.FromResult(response);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
