using FluentAssertions;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Tests;

public sealed class GameIdTests
{
    [Fact]
    public void Constructor_ShouldNormalizeValue()
    {
        var gameId = new GameId(" rmcP01 ");

        gameId.Value.Should().Be("RMCP01");
        gameId.ToString().Should().Be("RMCP01");
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("ABCDEF1")]
    [InlineData("ABC-01")]
    public void Constructor_ShouldRejectInvalidValue(string value)
    {
        var act = () => new GameId(value);

        act.Should().Throw<ArgumentException>();
    }
}
