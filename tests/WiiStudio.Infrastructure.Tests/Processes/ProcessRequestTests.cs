using FluentAssertions;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Tests.Processes;

public sealed class ProcessRequestTests
{
    [Fact]
    public void Constructor_ShouldRejectEmptyExecutablePath()
    {
        var act = () => new ProcessRequest(string.Empty, [], TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldRejectNonPositiveTimeout()
    {
        var act = () => new ProcessRequest("wit", [], TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
