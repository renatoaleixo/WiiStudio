using System.Net;
using System.Text;
using System.IO.Compression;
using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.GameTdb;
using WiiStudio.Infrastructure.Metadata;

namespace WiiStudio.Infrastructure.Tests.GameTdb;

public sealed class GameTdbUpdateSourceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"wiistudio-gametdb-source-{Guid.NewGuid():N}");

    [Fact]
    public async Task GetLatestAsync_ShouldUseRemoteHeaders()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Head);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content.Headers.LastModified = new DateTimeOffset(2026, 7, 10, 12, 30, 0, TimeSpan.Zero);
            return response;
        });
        var source = CreateSource(handler);

        var latest = await source.GetLatestAsync(CancellationToken.None);

        latest.Should().NotBeNull();
        latest!.Version.Should().Be("2026.07.10.123000");
        latest.Mirror.Should().Be("https://example.test/wiitdb.zip");
    }

    [Fact]
    public async Task DownloadAsync_ShouldValidateZipParseMetadataAndPersistCurrentDatabase()
    {
        var zip = CreateZip("""
            <?xml version="1.0" encoding="utf-8"?>
            <datafile version="2026.07.10">
              <game name="Mario Kart Wii">
                <id>RMCP01</id>
                <region>PAL</region>
                <developer>Nintendo</developer>
                <publisher>Nintendo</publisher>
                <date year="2008" />
                <locale lang="PT">
                  <title>Mario Kart Wii</title>
                  <synopsis>Corridas no Wii.</synopsis>
                </locale>
                <genre>Corrida</genre>
                <input><players>4</players></input>
              </game>
            </datafile>
            """);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(zip)
        });
        var source = CreateSource(handler);

        var package = await source.DownloadAsync(
            new GameTdbUpdateInfo("remote", DateTimeOffset.UtcNow, "https://example.test/wiitdb.zip"),
            new GameTdbUpdateOptions(Force: true),
            CancellationToken.None);

        package.Should().NotBeNull();
        package!.Info.Version.Should().Be("2026.07.10");
        package.Metadata.Should().ContainSingle();
        package.Metadata[0].GameId.Should().Be("RMCP01");
        package.Metadata[0].Developer.Should().Be("Nintendo");
        File.Exists(Path.Combine(root, "current", "wiitdb.xml")).Should().BeTrue();
        File.Exists(Path.Combine(root, "current", "manifest.json")).Should().BeTrue();
        File.Exists(Path.Combine(root, "wiitdb.xml")).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadAsync_ShouldRejectZipWithoutWiiTdbXml()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(CreateZip("<root />", "other.xml"))
        });
        var source = CreateSource(handler);

        var package = await source.DownloadAsync(
            new GameTdbUpdateInfo("remote", DateTimeOffset.UtcNow, "https://example.test/wiitdb.zip"),
            new GameTdbUpdateOptions(Force: true),
            CancellationToken.None);

        package.Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private GameTdbUpdateSource CreateSource(HttpMessageHandler handler)
    {
        return new GameTdbUpdateSource(
            new HttpClient(handler),
            new GameTdbMetadataProviderOptions
            {
                DatabaseDirectory = root,
                DatabaseUrl = "https://example.test/wiitdb.zip",
                PreferredLanguage = "PT",
                FallbackLanguages = ["EN"],
                Timeout = TimeSpan.FromSeconds(5)
            },
            new GameTdbMetadataMapper());
    }

    private static byte[] CreateZip(string xml, string entryName = "wiitdb.xml")
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(entryName);
            using var stream = entry.Open();
            var bytes = Encoding.UTF8.GetBytes(xml);
            stream.Write(bytes);
        }

        return memory.ToArray();
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            this.handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(handler(request));
        }
    }
}
