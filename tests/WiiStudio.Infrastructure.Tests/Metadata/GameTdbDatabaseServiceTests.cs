using FluentAssertions;
using System.IO.Compression;
using System.Net;
using System.Text;
using WiiStudio.Infrastructure.Metadata;

namespace WiiStudio.Infrastructure.Tests.Metadata;

public sealed class GameTdbDatabaseServiceTests
{
    [Fact]
    public async Task EnsureDatabaseAsync_ShouldRejectHttpDatabaseUrl()
    {
        using var fixture = TemporaryDatabaseFixture.Create();
        var service = fixture.CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK), new GameTdbMetadataProviderOptions
        {
            DatabaseDirectory = fixture.RootDirectory,
            DatabaseUrl = "http://www.gametdb.com/wiitdb.zip"
        });

        var status = await service.EnsureDatabaseAsync(forceRefresh: true, CancellationToken.None);

        status.IsAvailable.Should().BeFalse();
        status.Message.Should().Contain("HTTPS");
    }

    [Fact]
    public async Task EnsureDatabaseAsync_ShouldRejectZipWithoutDatabaseXml()
    {
        using var fixture = TemporaryDatabaseFixture.Create();
        var service = fixture.CreateService(_ => CreateZipResponse("readme.txt", "nope"));

        var status = await service.EnsureDatabaseAsync(forceRefresh: true, CancellationToken.None);

        status.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureDatabaseAsync_ShouldReuseFreshLocalDatabase()
    {
        using var fixture = TemporaryDatabaseFixture.Create();
        Directory.CreateDirectory(fixture.RootDirectory);
        await File.WriteAllTextAsync(Path.Combine(fixture.RootDirectory, "wiitdb.xml"), "<datafile />");
        await File.WriteAllTextAsync(Path.Combine(fixture.RootDirectory, "manifest.json"),
            $$"""
            {
              "Provider": "GameTDB",
              "DatabaseUrl": "https://www.gametdb.com/wiitdb.zip",
              "DownloadedAt": "{{DateTimeOffset.UtcNow:O}}",
              "PreferredLanguage": "PT",
              "GameCount": 0,
              "SourceVersion": "test"
            }
            """);
        var handlerCallCount = 0;
        var service = fixture.CreateService(_ =>
        {
            handlerCallCount++;
            return CreateZipResponse("wiitdb.xml", "<datafile />");
        });

        var status = await service.EnsureDatabaseAsync(forceRefresh: false, CancellationToken.None);

        status.IsAvailable.Should().BeTrue();
        handlerCallCount.Should().Be(0);
    }

    private static HttpResponseMessage CreateZipResponse(string entryName, string content)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new ByteArrayContent(CreateZip(entryName, content));
        return response;
    }

    private static byte[] CreateZip(string entryName, string content)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(entryName);
            using var stream = entry.Open();
            stream.Write(Encoding.UTF8.GetBytes(content));
        }

        return memory.ToArray();
    }

    private sealed class TemporaryDatabaseFixture : IDisposable
    {
        private TemporaryDatabaseFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public string RootDirectory { get; }

        public static TemporaryDatabaseFixture Create()
        {
            return new TemporaryDatabaseFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-gametdb-db-{Guid.NewGuid():N}"));
        }

        public GameTdbDatabaseService CreateService(
            Func<HttpRequestMessage, HttpResponseMessage> handler,
            GameTdbMetadataProviderOptions? options = null)
        {
            options ??= new GameTdbMetadataProviderOptions
            {
                DatabaseDirectory = RootDirectory,
                DatabaseUrl = "https://www.gametdb.com/wiitdb.zip"
            };

            return new GameTdbDatabaseService(new HttpClient(new FakeHttpMessageHandler(handler)), options);
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
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
}
