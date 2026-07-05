using FluentAssertions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Tests;

public sealed class GameTests
{
    [Fact]
    public void Constructor_ShouldTrimValues()
    {
        var game = new Game(" RMCP01 ", " Mario Kart Wii ", GameFormat.Wbfs, 1024);

        game.Id.Should().Be("RMCP01");
        game.Title.Should().Be("Mario Kart Wii");
        game.GameId.Should().Be(new GameId("RMCP01"));
        game.Format.Should().Be(FileFormat.Wbfs);
        game.SizeInBytes.Should().Be(1024);
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeSize()
    {
        var act = () => new Game("RMCP01", "Mario Kart Wii", GameFormat.Wbfs, -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
