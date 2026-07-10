using FluentAssertions;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Wiimms;

public sealed class WiimmsProgressParserTests
{
    [Fact]
    public void TryParse_ShouldParsePercentage()
    {
        var parser = new WiimmsProgressParser();

        var parsed = parser.TryParse("copying RMCP01 43.5%", out var progress);

        parsed.Should().BeTrue();
        progress.Percentage.Should().Be(43.5);
        progress.IsIndeterminate.Should().BeFalse();
        progress.CurrentOperation.Should().Contain("copying");
    }

    [Fact]
    public void TryParse_ShouldParseBytesAndSpeed()
    {
        var parser = new WiimmsProgressParser();

        var parsed = parser.TryParse("123 MiB / 246 MiB 12 MiB/s", out var progress);

        parsed.Should().BeTrue();
        progress.ProcessedBytes.Should().Be(123L * 1024L * 1024L);
        progress.TotalBytes.Should().Be(246L * 1024L * 1024L);
        progress.SpeedBytesPerSecond.Should().Be(12L * 1024L * 1024L);
        progress.EstimatedRemaining.Should().NotBeNull();
    }

    [Fact]
    public void TryParse_ShouldIgnoreIrrelevantLine()
    {
        var parser = new WiimmsProgressParser();

        var parsed = parser.TryParse("ID6     12/500 discs", out _);

        parsed.Should().BeFalse();
    }
}
