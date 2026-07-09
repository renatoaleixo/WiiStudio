using FluentAssertions;
using System.IO.Compression;
using System.Net;
using System.Text;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Metadata;

namespace WiiStudio.Infrastructure.Tests.Metadata;

public sealed class GameTdbMetadataProviderTests
{
    [Fact]
    public async Task GetByGameIdAsync_ShouldDownloadDatabaseAndMapMetadata()
    {
        using var fixture = TemporaryGameTdbFixture.Create();
        var handler = new FakeHttpMessageHandler(_ => CreateZipResponse(CreateXml()));
        var options = fixture.CreateOptions();
        var provider = CreateProvider(handler, options);

        var metadata = await provider.GetByGameIdAsync("rmcp01", CancellationToken.None);

        metadata.Should().NotBeNull();
        metadata!.GameId.Should().Be("RMCP01");
        metadata.Title.Should().Be("Mario Kart Wii PT");
        metadata.Region.Should().Be(Region.Pal);
        metadata.Developer.Should().Be("Nintendo EAD");
        metadata.Publisher.Should().Be("Nintendo");
        metadata.Year.Should().Be(2008);
        metadata.Players.Should().Be(4);
        metadata.Genres.Should().Contain("Racing");
        metadata.Languages.Should().Contain("PT");
        metadata.Description.Should().Be("Descricao em portugues.");
        metadata.GameTdbVersion.Should().Be("2026.07.09");
        File.Exists(Path.Combine(options.DatabaseDirectory, "wiitdb.xml")).Should().BeTrue();
    }

    [Fact]
    public async Task GetByGameIdAsync_ShouldFallbackToEnglishWhenPreferredLocaleIsMissing()
    {
        using var fixture = TemporaryGameTdbFixture.Create();
        var handler = new FakeHttpMessageHandler(_ => CreateZipResponse(CreateXml(includePortuguese: false)));
        var provider = CreateProvider(handler, fixture.CreateOptions());

        var metadata = await provider.GetByGameIdAsync("RMCP01", CancellationToken.None);

        metadata.Should().NotBeNull();
        metadata!.Title.Should().Be("Mario Kart Wii");
        metadata.Description.Should().Be("English description.");
    }

    [Fact]
    public async Task GetByGameIdAsync_ShouldReturnNullWhenGameIsMissing()
    {
        using var fixture = TemporaryGameTdbFixture.Create();
        var handler = new FakeHttpMessageHandler(_ => CreateZipResponse(CreateXml()));
        var provider = CreateProvider(handler, fixture.CreateOptions());

        var metadata = await provider.GetByGameIdAsync("RSPE01", CancellationToken.None);

        metadata.Should().BeNull();
    }

    [Fact]
    public async Task GetByGameIdAsync_ShouldReturnNullWhenDatabaseCannotBeDownloaded()
    {
        using var fixture = TemporaryGameTdbFixture.Create();
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var provider = CreateProvider(handler, fixture.CreateOptions());

        var metadata = await provider.GetByGameIdAsync("RMCP01", CancellationToken.None);

        metadata.Should().BeNull();
    }

    [Fact]
    public async Task GetByGameIdAsync_ShouldRejectInvalidGameId()
    {
        using var fixture = TemporaryGameTdbFixture.Create();
        var handler = new FakeHttpMessageHandler(_ => CreateZipResponse(CreateXml()));
        var provider = CreateProvider(handler, fixture.CreateOptions());

        var act = () => provider.GetByGameIdAsync("../bad", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static GameTdbMetadataProvider CreateProvider(FakeHttpMessageHandler handler, GameTdbMetadataProviderOptions options)
    {
        var database = new GameTdbDatabaseService(new HttpClient(handler), options);
        return new GameTdbMetadataProvider(database, new GameTdbXmlParser(), new GameTdbMetadataMapper(), options);
    }

    private static HttpResponseMessage CreateZipResponse(string xml)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new ByteArrayContent(CreateZip(xml));
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        return response;
    }

    private static byte[] CreateZip(string xml)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("wiitdb.xml");
            using var stream = entry.Open();
            var bytes = Encoding.UTF8.GetBytes(xml);
            stream.Write(bytes);
        }

        return memory.ToArray();
    }

    private static string CreateXml(bool includePortuguese = true)
    {
        var portuguese = includePortuguese
            ? """
                  <locale lang="PT">
                    <title>Mario Kart Wii PT</title>
                    <synopsis>Descricao em portugues.</synopsis>
                  </locale>
              """
            : string.Empty;

        return $$"""
            <?xml version="1.0" encoding="UTF-8"?>
            <datafile version="2026.07.09">
              <game name="Mario Kart Wii Internal">
                <id>RMCP01</id>
                <region>PAL</region>
                <developer>Nintendo EAD</developer>
                <publisher>Nintendo</publisher>
                <date year="2008" month="04" day="11" />
                <languages>EN, PT, ES</languages>
                <genre>Racing, Party</genre>
                <input players="4" />
                <rating type="PEGI" value="3" />
                <wi-fi players="12">
                  <feature>Online racing</feature>
                </wi-fi>
                {{portuguese}}
                <locale lang="EN">
                  <title>Mario Kart Wii</title>
                  <synopsis>English description.</synopsis>
                </locale>
              </game>
            </datafile>
            """;
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    private sealed class TemporaryGameTdbFixture : IDisposable
    {
        private TemporaryGameTdbFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public string RootDirectory { get; }

        public static TemporaryGameTdbFixture Create()
        {
            return new TemporaryGameTdbFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-gametdb-metadata-{Guid.NewGuid():N}"));
        }

        public GameTdbMetadataProviderOptions CreateOptions()
        {
            return new GameTdbMetadataProviderOptions
            {
                DatabaseDirectory = RootDirectory,
                DatabaseUrl = "https://www.gametdb.com/wiitdb.zip",
                PreferredLanguage = "PT",
                FallbackLanguages = ["PT", "EN", "US", "ORIG"]
            };
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }
}
