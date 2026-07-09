using FluentAssertions;
using System.Text;
using WiiStudio.Infrastructure.Metadata;

namespace WiiStudio.Infrastructure.Tests.Metadata;

public sealed class GameTdbXmlParserTests
{
    [Fact]
    public async Task FindByGameIdAsync_ShouldBlockDtd()
    {
        const string xml = """
            <!DOCTYPE datafile [
              <!ENTITY xxe SYSTEM "file:///etc/passwd">
            ]>
            <datafile><game><id>RMCP01</id><locale lang="EN"><title>&xxe;</title></locale></game></datafile>
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var parser = new GameTdbXmlParser();

        var act = () => parser.FindByGameIdAsync(stream, "RMCP01", CancellationToken.None);

        await act.Should().ThrowAsync<System.Xml.XmlException>();
    }
}
